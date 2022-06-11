using Domain.Persistables;
using Domain.Services.Injections;
using Domain.Services.Orders;
using Domain.Services.ShippingWarehouses;
using Domain.Services.Warehouses;
using Domain.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.EntityFrameworkCore.Internal;
using Tasks.Common;
using Tasks.Helpers;
using Domain.Extensions;
using Domain.Enums;
using Tasks.Statistics;

namespace Tasks.Orders
{
    [Description("Импорт инжекций на создание нового заказа")]
    public class ImportOrderTask : TaskBase<ImportOrderProperties>, IScheduledTask
    {
        public string Schedule => "*/5 * * * *";

        protected override async Task Execute(IServiceProvider serviceProvider, ImportOrderProperties props, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(props.ConnectionString))
            {
                throw new Exception("ConnectionString является обязательным параметром");
            }

            if (string.IsNullOrEmpty(props.Folder))
            {
                props.Folder = "/";
            }

            if (string.IsNullOrEmpty(props.FileNamePattern))
            {
                props.FileNamePattern = @"^.*ORD.*\.xml$";
            }

            if (string.IsNullOrEmpty(props.ViewHours))
            {
                props.ViewHours = "24";
            }

            int viewHours;
            if (!int.TryParse(props.ViewHours, out viewHours))
            {
                throw new Exception("Параметр ViewHours должен быть целым числом");
            }

            try
            {
                Regex fileNameRe = new Regex(props.FileNamePattern, RegexOptions.IgnoreCase);
                IInjectionsService injectionsService = serviceProvider.GetService<IInjectionsService>();

                ConnectionInfo sftpConnection = GetSftpConnection(props.ConnectionString);
                using (SftpClient sftpClient = new SftpClient(sftpConnection))
                {
                    sftpClient.Connect();

                    DateTime barrierTime = DateTime.UtcNow.AddHours(-viewHours);
                    IEnumerable<InjectionDto> processedInjections = injectionsService.GetByTaskName(TaskName);
                    HashSet<string> processedFileNames = new HashSet<string>(processedInjections.Select(i => i.FileName));

                    var files = sftpClient.ListDirectory(props.Folder);
                    files = files.Where(f => f.LastWriteTimeUtc >= barrierTime && f.IsRegularFile)
                                 .OrderBy(f => f.LastWriteTimeUtc);

                    var filesQueueLength = files.Count();

                    foreach (SftpFile file in files)
                    {
                        StatisticsStore.UpdateFilesQueueLength(TaskName, filesQueueLength);
                        --filesQueueLength;

                        if (!fileNameRe.IsMatch(file.Name))
                        {
                            continue;
                        }

                        if (!processedFileNames.Contains(file.Name))
                        {
                            Log.Information("Найден новый файл: {FullName}.", file.FullName);

                            InjectionDto injection = new InjectionDto
                            {
                                Type = TaskName,
                                FileName = file.Name,
                                ProcessTimeUtc = DateTime.UtcNow
                            };

                            try
                            {
                                string content = sftpClient.ReadAllText(file.FullName);
                                bool isSuccess = ProcessOrderFile(serviceProvider, file.Name, content);
                                injection.Status = isSuccess ? InjectionStatus.Success : InjectionStatus.Failed;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Не удалось обработать файл {Name}.", file.Name);
                                injection.Status = InjectionStatus.Failed;
                            }

                            injectionsService.SaveOrCreate(injection, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Ошибка при обработке {TaskName} инжекции.");
                throw ex;
            }
        }

        private ConnectionInfo GetSftpConnection(string connectionString)
        {
            Uri connection = new Uri(connectionString);
            string[] authParts = connection.UserInfo.Split(':');
            string login = authParts.Length == 2 ? HttpUtility.UrlDecode(authParts[0]) : null;
            string password = authParts.Length == 2 ? HttpUtility.UrlDecode(authParts[1]) : null;
            int port = connection.Port > 0 ? connection.Port : 22;
            ConnectionInfo result = new ConnectionInfo(connection.Host, port, login, new PasswordAuthenticationMethod(login, password));
            return result;
        }

        private bool ProcessOrderFile(IServiceProvider serviceProvider, string fileName, string fileContent)
        {
            IWarehousesService warehousesService = serviceProvider.GetService<IWarehousesService>();
            IShippingWarehousesService shippingWarehousesService = serviceProvider.GetService<IShippingWarehousesService>();
            IOrdersService ordersService = serviceProvider.GetService<IOrdersService>();

            // Загружаем данные из файла
            XmlDocument doc = new XmlDocument();
            using (StringReader reader = new StringReader(fileContent))
            {
                doc.Load(reader);
            }

            List<WarehouseDto> updWarehouses = new List<WarehouseDto>();
            var docRoots = doc.SelectNodes("//IDOC");

            int totalCount = docRoots.Count;
            var entriesQueue = totalCount;
            int processedCount = 0;

            
            var orders = new List<OrderFormDto>();
            
            foreach (XmlNode docRoot in docRoots)
            {
                StatisticsStore.UpdateEntriesQueueLength(TaskName, entriesQueue);
                --entriesQueue;

                ++processedCount;

                string orderNumber = docRoot.SelectSingleNode("E1EDK02[QUALF='002']/BELNR")?.InnerText?.TrimStart('0');
                if (orderNumber?.Contains('@') == true)
                {
                    Log.Warning("Номер накладной BDF {orderNumber} соответствует рекламной продукции. Заказ ({processedCount}/{totalCount}) не создан.", 
                                orderNumber, processedCount, totalCount);
                    continue;
                }

                OrderFormDto dto = ordersService.GetFormByNumber(orderNumber);
                bool isNew = dto == null;
                if (dto == null)
                {
                    dto = new OrderFormDto
                    {
                        AmountConfirmed = false,
                        DocumentAttached = false,
                        DocumentReturnStatus = false,
                        Invoice = false,
                        IsActive = true,
                        OrderConfirmed = false,
                        WaybillTorg12 = false
                    };
                }

                dto.AdditionalInfo = $"INJECTION - {fileName}";

                bool isPoolingBooked = string.Equals(dto.OrderShippingStatus, ShippingState.ShippingSlotBooked.ToString(), StringComparison.InvariantCultureIgnoreCase);

                var deliveryType = dto.DeliveryType?.Value?.ToEnum<DeliveryType>();
                bool isShippingSoon = deliveryType == DeliveryType.Delivery || deliveryType == DeliveryType.SelfDelivery;

                decimal weightUomCoeff = docRoot.ParseUom("E1EDK01/GEWEI", new[] { "GRM", "GR", "KGM", "KG" }, new[] { 0.001M, 0.001M, 1M, 1M }, 1);

                dto.OrderNumber = orderNumber;
                dto.OrderDate = docRoot.ParseDateTime("E1EDK02[QUALF='001']/DATUM")?.FormatDate() ?? dto.OrderDate;

                if (!isPoolingBooked)
                {
                    string soldTo = docRoot.SelectSingleNode("E1EDKA1[PARVW='AG']/PARTN")?.InnerText?.TrimStart('0');
                    string deliveryDate = docRoot.ParseDateTime("E1EDK03[IDDAT='002']/DATUM")?.FormatDate();

                    bool isDeliveryDateChanged = !dto.ManualDeliveryDate && !string.IsNullOrEmpty(deliveryDate) && deliveryDate.ToDate() != dto.DeliveryDate.ToDate();

                    if (isDeliveryDateChanged || soldTo != dto.SoldTo)
                    {
                        dto.ShippingDate = null;
                    }

                    dto.SoldTo = soldTo ?? dto.SoldTo;
                    dto.DeliveryDate = isDeliveryDateChanged ? deliveryDate : dto.DeliveryDate;
                    dto.OrderAmountExcludingVAT = docRoot.ParseDecimal("E1EDS01[SUMID='002']/SUMME") ?? dto.OrderAmountExcludingVAT;
                    
                    string shippingAddressCode = docRoot.SelectSingleNode("E1EDP01/WERKS")?.InnerText;
                    var shippingWarehouse = shippingWarehousesService.GetByCode(shippingAddressCode);
                    dto.ShippingAddress = shippingWarehouse?.Address ?? dto.ShippingAddress;
                    dto.ShippingCity = shippingWarehouse?.City;
                    dto.ShippingWarehouseId = shippingWarehouse?.Id == null ? dto.ShippingWarehouseId : new LookUpDto(shippingWarehouse.Id.FormatGuid(), shippingWarehouse.WarehouseName);

                    var deliveryWarehouse = warehousesService.GetBySoldTo(dto.SoldTo);
                    if (deliveryWarehouse == null)
                    {
                        dto.ClientName = null;
                        dto.DeliveryAddress = null;
                        dto.DeliveryCity = null;
                        dto.DeliveryRegion = null;
                        dto.PickingTypeId = null;
                        dto.TransitDays = null;
                        dto.DeliveryType = null;
                    }

                    DateTime? shippingDate = isDeliveryDateChanged ? dto.DeliveryDate.ToDate()?.AddDays(0 - dto.TransitDays ?? 0) : dto.ShippingDate.ToDate();
                    isShippingSoon &= shippingDate != null && DateTime.Now > shippingDate.Value.Date.AddDays(-1).AddHours(13);

                    if (!isShippingSoon)
                    {
                        dto.WeightKg = docRoot.ParseDecimal("E1EDK01/BRGEW").ApplyDecimalUowCoeff(weightUomCoeff) ?? dto.WeightKg;
                        dto.BoxesCount = docRoot.ParseDecimal("E1EDK01/Y0126SD_ORDERS05_TMS_01/YYCAR_H") ?? dto.BoxesCount;
                    }
                }
                else
                {
                    DateTime? shippingDate = dto.ShippingDate.ToDate();
                    isShippingSoon &= shippingDate != null && DateTime.Now > shippingDate.Value.Date.AddDays(-1).AddHours(13);
                }

                if (isNew)
                {
                    dto.ClientOrderNumber = docRoot.SelectSingleNode("E1EDK02[QUALF='001']/BELNR")?.InnerText ?? dto.ClientOrderNumber;
                    dto.Payer = docRoot.SelectSingleNode("E1EDKA1[PARVW='RG']/PARTN")?.InnerText?.TrimStart('0') ?? dto.Payer;
                }

                if (isNew || (dto.ManualPalletsCount != true && !isPoolingBooked && !isShippingSoon))
                {
                    dto.PalletsCount = docRoot.ParseInt("E1EDK01/Y0126SD_ORDERS05_TMS_01/YYPAL_H") ?? dto.PalletsCount;
                }

                IEnumerable<string> missedRequiredFields = ValidateRequiredFields(dto);
                if (missedRequiredFields.Any())
                {
                    string fields = string.Join(", ", missedRequiredFields);
                    Log.Error("В файле {fileName} отсутствуют следующие обязательные поля: {fields}. Заказ ({processedCount}/{totalCount}) не создан.",
                              fileName, fields, processedCount, totalCount);
                }
                else
                {
                    int entryInd = 0;
                    var itemRoots = docRoot.SelectNodes("E1EDP01");
                    dto.Items = dto.Items ?? new List<OrderItemDto>();
                    var updatedItems = new HashSet<string>();
                    foreach (XmlNode itemRoot in itemRoots)
                    {
                        ++entryInd;

                        string posex = itemRoot.SelectSingleNode("POSEX")?.InnerText ?? string.Empty;
                        int posexNum = -1;
                        int.TryParse(posex.TrimStart('0'), out posexNum);
                        if ((posexNum % 10) != 0)
                        {
                            continue;
                        }

                        string nart = itemRoot.SelectSingleNode("E1EDP19/IDTNR")?.InnerText?.TrimStart('0');
                        if (string.IsNullOrEmpty(nart))
                        {
                            Log.Warning("Пустое значение NART в позиции #{entryInd} заказа ({processedCount}/{totalCount}) из файла {fileName}, пропуск.",
                                        entryInd, processedCount, totalCount, fileName);
                            continue;
                        }

                        int? quantity = itemRoot.ParseInt("MENGE");
                        if (quantity == null || quantity == 0)
                        {
                            Log.Warning("Пустое количество в позиции #{entryInd} заказа ({processedCount}/{totalCount}) из файла {fileName}, пропуск.",
                                        entryInd, processedCount, totalCount, fileName);
                            continue;
                        }

                        OrderItemDto itemDto = dto.Items.Where(i => i.Nart == nart).FirstOrDefault();

                        if (itemDto == null)
                        {
                            itemDto = new OrderItemDto();
                            dto.Items.Add(itemDto);
                        }
                        else
                        {
                            updatedItems.Add(itemDto.Id);
                        }

                        itemDto.Nart = nart;
                        itemDto.Quantity = quantity ?? itemDto.Quantity;
                    }

                    var itemsToRemove = dto.Items.Where(x => !string.IsNullOrEmpty(x.Id) && !updatedItems.Contains(x.Id)).ToList();
                    itemsToRemove.ForEach(x => dto.Items.Remove(x));

                    if (isNew)
                    {
                        Log.Information("Создан новый заказ {OrderNumber} ({processedCount}/{totalCount}) на основании файла {fileName}.",
                                        dto.OrderNumber, processedCount, totalCount, fileName);
                    }
                    else
                    {
                        Log.Information("Обновлен заказ {OrderNumber} ({processedCount}/{totalCount}) на основании файла {fileName}.",
                                        dto.OrderNumber, processedCount, totalCount, fileName);
                    }

                    orders.Add(dto);
                }
            }

            bool isSuccess = orders.Any();
            if (isSuccess)
            {
                var importResults = ordersService.Import(orders);
                if (importResults != null)
                {
                    foreach (var importResult in importResults)
                    {
                        if (importResult.IsError)
                        {
                            Log.Warning("Ошибка при импорте данных заказа на основании файла {fileName}: {Error}", fileName, importResult.Message);
                        }
                    }
                }
            }

            return isSuccess;
        }

        private IEnumerable<string> ValidateRequiredFields(OrderDto dto)
        {
            if (string.IsNullOrEmpty(dto.OrderNumber))
            {
                yield return "Номер накладной BDF";
            }
            if (string.IsNullOrEmpty(dto.ClientOrderNumber))
            {
                yield return "Номер заказа клиента";
            }
            if (dto.OrderDate == null)
            {
                yield return "Дата заказа";
            }
            if (string.IsNullOrEmpty(dto.Payer))
            {
                yield return "Плательщик";
            }
            if (string.IsNullOrEmpty(dto.SoldTo))
            {
                yield return "Sold-to";
            }
        }
    }
}
