using Application.Shared.Excel;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.FieldProperties;
using Domain.Services.Orders;
using Domain.Services.Orders.Import;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Application.Services.Orders.Import
{
    public class OrdersImportService : IOrdersImportService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IOrdersService _ordersService;
        private readonly ExcelMapper<OrdersImportDto> _excelMapper;

        private const string _shippingWarehousesSheetName = "ShippingWarehouses";
        private const string _deliveryWarehousesSheetName = "DeliveryWarehouses";
        private const string _deliveryAddressesSheetName = "DeliveryAddresses";
        private const string _ordersSheetName = "Orders";

        public OrdersImportService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IOrdersService ordersService,
            IFieldDispatcherService fieldDispatcher)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _ordersService = ordersService;
            _excelMapper = new ExcelMapper<OrdersImportDto>(dataService, userProvider, fieldDispatcher);
        }

        public Stream GenerateExcelTemplate()
        {
            var excel = new ExcelPackage();

            FillReferences(excel);
            FillDataSheet(excel);

            return new MemoryStream(excel.GetAsByteArray());
        }

        public OperationDetailedResult ImportFromExcel(Stream fileStream, string fileName)
        {
            var result = new OperationDetailedResult();

            try
            {
                var excel = new ExcelPackage(fileStream);

                var user = _userProvider.GetCurrentUser();
                var lang = user?.Language;

                result.Message = "ordersImportTitle".Translate(lang);

                var dataSheet = excel.Workbook.Worksheets[_ordersSheetName];

                if (dataSheet == null)
                {
                    result.Entries.Add(new OperationDetailedResultItem
                    {
                        Title = "invalidFileFormatError".Translate(lang),
                        IsError = true
                    });

                    return result;
                }

                var entries = _excelMapper.LoadEntries(dataSheet, lang);

                if (!entries.Any())
                {
                    result.Entries.Add(new OperationDetailedResultItem
                    {
                        Title = "emptyFileError".Translate(lang),
                        IsError = true
                    });

                    return result;
                }

                HashSet<string> existOrders = LoadExistOrderNumbers(entries);
                Dictionary<string, ShippingWarehouse> shippingWarehousesDict = LoadShippingWarehouses();
                Dictionary<string, Warehouse> deliveryWarehousesDict = LoadDeliveryWarehouses();

                var successNumbers = new List<string>();
                var errorMessages = new List<string>();
                var emptyNumberLineNumbers = new List<string>();
                var emptyDataNumbers = new List<string>();
                var existOrderNumbers = new List<string>();
                var invalidDatesNumbers = new List<string>();
                var duplicateLineNumbers = new List<string>();

                var orderFormDtos = new List<OrderFormDto>();

                var updatedShippings = new HashSet<string>();

                foreach (var entry in entries)
                {
                    if (entry.Data == null)
                    {
                        continue;
                    }

                    if (entry.Result.IsError)
                    {
                        errorMessages.Add(entry.Result.Message);
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.Data.OrderNumber))
                    {
                        emptyNumberLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }

                    if (existOrders.Contains(entry.Data.OrderNumber))
                    {
                        existOrderNumbers.Add(entry.Data.OrderNumber);
                        continue;
                    }

                    var shippingDate = entry.Data.ShippingDate.ToDate();
                    var shippingTime = entry.Data.ShippingTime.ToTime();
                    var deliveryDate = entry.Data.DeliveryDate.ToDate();
                    var deliveryTime = entry.Data.DeliveryTime.ToTime();

                    if (!string.IsNullOrEmpty(entry.Data.ShippingDate) && shippingDate == null)
                    {
                        errorMessages.Add("importLineError".Translate(lang, entry.RecordNumber, "invalidDateValueFormat".Translate(lang, entry.Data.ShippingDate)));
                        continue;
                    }

                    if (!string.IsNullOrEmpty(entry.Data.ShippingTime) && shippingTime == null)
                    {
                        errorMessages.Add("importLineError".Translate(lang, entry.RecordNumber, "invalidTimeValueFormat".Translate(lang, entry.Data.ShippingTime)));
                        continue;
                    }

                    if (!string.IsNullOrEmpty(entry.Data.DeliveryDate) && deliveryDate == null)
                    {
                        errorMessages.Add("importLineError".Translate(lang, entry.RecordNumber, "invalidDateValueFormat".Translate(lang, entry.Data.DeliveryDate)));
                        continue;
                    }

                    if (!string.IsNullOrEmpty(entry.Data.DeliveryTime) && deliveryTime == null)
                    {
                        errorMessages.Add("importLineError".Translate(lang, entry.RecordNumber, "invalidTimeValueFormat".Translate(lang, entry.Data.DeliveryTime)));
                        continue;
                    }

                    if (!string.IsNullOrEmpty(entry.Data.ShippingWarehouseName) && !shippingWarehousesDict.ContainsKey(entry.Data.ShippingWarehouseName))
                    {
                        errorMessages.Add("importLineError".Translate(lang, entry.RecordNumber, "invalidReferenceValueFormat".Translate(lang, entry.Data.ShippingWarehouseName)));
                        continue;
                    }

                    if (!string.IsNullOrEmpty(entry.Data.DeliveryWarehouseName) && !deliveryWarehousesDict.ContainsKey(entry.Data.DeliveryWarehouseName))
                    {
                        errorMessages.Add("importLineError".Translate(lang, entry.RecordNumber, "invalidReferenceValueFormat".Translate(lang, entry.Data.DeliveryWarehouseName)));
                        continue;
                    }

                    if (deliveryDate == null
                        || deliveryTime == null
                        || entry.Data.PalletsCount == null
                        || shippingDate == null
                        || shippingTime == null
                        || string.IsNullOrEmpty(entry.Data.ShippingWarehouseName)
                        || (string.IsNullOrEmpty(entry.Data.DeliveryWarehouseName) && string.IsNullOrEmpty(entry.Data.DeliveryAddress))
                        || entry.Data.WeightKg == null)
                    {
                        emptyDataNumbers.Add(entry.Data.OrderNumber);
                        continue;
                    }

                    if (shippingDate + shippingTime > deliveryDate + deliveryTime)
                    {
                        invalidDatesNumbers.Add(entry.Data.OrderNumber);
                        continue;
                    }

                    if (updatedShippings.Contains(entry.Data.OrderNumber))
                    {
                        duplicateLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }
                    updatedShippings.Add(entry.Data.OrderNumber);

                    shippingWarehousesDict.TryGetValue(entry.Data.ShippingWarehouseName, out ShippingWarehouse shippingWarehouse);

                    var orderFormDto = new OrderFormDto
                    {
                        OrderNumber = entry.Data.OrderNumber,
                        ClientOrderNumber = entry.Data.ClientOrderNumber,
                        ShippingDate = (shippingDate + shippingTime).FormatDateTime(),
                        DeliveryDate = (deliveryDate + deliveryTime).FormatDateTime(),
                        ShippingWarehouseId = shippingWarehouse != null ? new LookUpDto(shippingWarehouse.Id.FormatGuid(), shippingWarehouse.WarehouseName) : null,
                        ShippingAddress = shippingWarehouse?.Address,
                        PalletsCount = entry.Data.PalletsCount,
                        Volume = entry.Data.Volume,
                        WeightKg = entry.Data.WeightKg,
                        OrderAmountExcludingVAT = entry.Data.OrderAmountExcludingVAT
                    };

                    if (!string.IsNullOrEmpty(entry.Data.DeliveryWarehouseName)
                        && deliveryWarehousesDict.TryGetValue(entry.Data.DeliveryWarehouseName, out Warehouse warehouse))
                    {
                        orderFormDto.DeliveryWarehouseId = new LookUpDto(warehouse.Id.FormatGuid(), warehouse.ToString());
                        orderFormDto.DeliveryAddress = warehouse.Address;
                    }
                    else
                    {
                        orderFormDto.DeliveryAddress = entry.Data.DeliveryAddress?.Trim();
                    }

                    orderFormDtos.Add(orderFormDto);

                    successNumbers.Add(entry.Data.OrderNumber);
                }

                var importResult = _ordersService.Import(orderFormDtos);
                var importErrors = importResult.Where(i => i.IsError).ToList();

                if (importErrors.Any())
                {
                    errorMessages.AddRange(importErrors.Select(i => i.Message));
                }

                int totalCount = entries.Count();

                AddEntriesGroup(result, lang, totalCount, "ordersImportProcessed", successNumbers, false, 4);
                AddEntriesGroupLineNumbers(result, lang, totalCount, "ordersImportEmptyNumber", emptyNumberLineNumbers, true);
                AddEntriesGroup(result, lang, totalCount, "ordersImportEmptyData", emptyDataNumbers, true, 4);
                AddEntriesGroup(result, lang, totalCount, "ordersImportExistOrder", existOrderNumbers, true, 4);
                AddEntriesGroup(result, lang, totalCount, "ordersImportInvalidDates", invalidDatesNumbers, true, 4);
                AddEntriesGroupLineNumbers(result, lang, totalCount, "ordersImportOrderDuplicate", duplicateLineNumbers, true);
                AddEntriesGroup(result, lang, totalCount, "ordersImportErrorMessages", errorMessages, true, 1);

                _dataService.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Error = ex.Message;
                result.Message = null;
                result.Entries.Clear();
            }

            return result;
        }

        private void AddEntriesGroup(OperationDetailedResult result, string lang, int totalCount, 
                                     string titleKey, List<string> messages, bool isError, int columnsCount,
                                     int? recordsCount = null)
        {
            if (messages.Any())
            {
                result.Entries.Add(new OperationDetailedResultItem
                {
                    IsError = isError,
                    MessageColumns = columnsCount,
                    Messages = messages,
                    Title = titleKey.Translate(lang, recordsCount ?? messages.Count, totalCount)
                });
            }
        }

        private void AddEntriesGroupLineNumbers(OperationDetailedResult result, string lang, int totalCount,
                                                string titleKey, List<string> messages, bool isError)
        {
            if (messages.Any())
            {
                string lineNumbers = string.Join(", ", messages);
                string message = "importLinesList".Translate(lang, lineNumbers);
                AddEntriesGroup(result, lang, totalCount, titleKey, new List<string> { message }, isError, 1, messages.Count);
            }
        }

        private HashSet<string> LoadExistOrderNumbers(IEnumerable<ValidatedRecord<OrdersImportDto>> entries)
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;

            var orderNumbers = entries.Select(x => x.Data?.OrderNumber)
                                      .Where(x => !string.IsNullOrEmpty(x))
                                      .Distinct()
                                      .ToList();

            var orders = _dataService.GetDbSet<Order>()
                                     .Where(x => orderNumbers.Contains(x.OrderNumber)
                                            && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                     .Select(x => x.OrderNumber)
                                     .ToList();

            return orders.ToHashSet();
        }

        private Dictionary<string, ShippingWarehouse> LoadShippingWarehouses()
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            var shippingWarehouses = _dataService.GetDbSet<ShippingWarehouse>()
                                                 .Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                                 .ToList();

            var result = new Dictionary<string, ShippingWarehouse>();
            foreach (var warehouse in shippingWarehouses)
            {
                result[warehouse.WarehouseName] = warehouse;
            }

            return result;
        }

        private Dictionary<string, Warehouse> LoadDeliveryWarehouses()
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            var warehouses = _dataService.GetDbSet<Warehouse>()
                                         .Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                         .ToList();

            var result = new Dictionary<string, Warehouse>();
            foreach (var warehouse in warehouses)
            {
                result[warehouse.WarehouseName] = warehouse;
            }

            return result;
        }

        private void FillReferences(ExcelPackage excel)
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;

            var shippingWarehouses = _dataService.GetDbSet<ShippingWarehouse>()
                                           .Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                           .Select(x => x.WarehouseName)
                                           .OrderBy(x => x)
                                           .ToList();

            var deliveryWarehouses = _dataService.GetDbSet<Warehouse>()
                                           .Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                           .ToList();

            FillReferenceSheet(excel, _shippingWarehousesSheetName, shippingWarehouses);

            var deliveryWarehouseAddresses = deliveryWarehouses.Select(x => x.Address).OrderBy(x => x).ToList();
            FillReferenceSheet(excel, _deliveryAddressesSheetName, deliveryWarehouseAddresses);

            var deliveryWarehouseNames = deliveryWarehouses.Select(x => x.WarehouseName).OrderBy(x => x).ToList();
            FillReferenceSheet(excel, _deliveryWarehousesSheetName, deliveryWarehouseNames);
        }

        private void FillReferenceSheet(ExcelPackage excel, string sheetName, List<string> values)
        {
            var sheet = excel.Workbook.Worksheets.Add(sheetName);
            sheet.Hidden = eWorkSheetHidden.VeryHidden;

            for (int ind = 0; ind < values.Count; ind++)
            {
                sheet.Cells[ind + 1, 1].Value = values[ind];
            }
        }

        private void FillDataSheet(ExcelPackage excel)
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;

            var dataSheet = excel.Workbook.Worksheets.Add(_ordersSheetName);

            _excelMapper.FillSheet(dataSheet, new OrdersImportDto[0], lang);

            int columnsCount = _excelMapper.Columns.Count();
            dataSheet.Cells[1, 1, 1, columnsCount].Style.Font.Bold = true;
            for (int ind = 1; ind <= columnsCount; ind++)
            {
                dataSheet.Column(ind).Width = 20;
            }

            dataSheet.Cells["C:C"].Style.Numberformat.Format = dataSheet.Cells["E:E"].Style.Numberformat.Format = "dd/MM/yyyy";
            dataSheet.Cells["D:D"].Style.Numberformat.Format = dataSheet.Cells["F:F"].Style.Numberformat.Format = "hh:mm:ss;@";
            dataSheet.Cells["J:J"].Style.Numberformat.Format = "0";
            dataSheet.Cells["K:K"].Style.Numberformat.Format = "0.00";
            dataSheet.Cells["L:L"].Style.Numberformat.Format = "0.00";

            var swValidation = dataSheet.DataValidations.AddListValidation("G2:G10000");
            swValidation.ShowErrorMessage = true;
            swValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            swValidation.ErrorTitle = "listValidationTitle".Translate(lang);
            swValidation.Error = "listValidationMessage".Translate(lang);
            swValidation.Formula.ExcelFormula = $"{_shippingWarehousesSheetName}!A:A";

            var dwValidation = dataSheet.DataValidations.AddListValidation("H2:H10000");
            dwValidation.ShowErrorMessage = false;
            dwValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            dwValidation.ErrorTitle = "listValidationTitle".Translate(lang);
            dwValidation.Error = "listValidationMessage".Translate(lang);
            dwValidation.Formula.ExcelFormula = $"{_deliveryWarehousesSheetName}!A:A";

            var daValidation = dataSheet.DataValidations.AddListValidation("I2:I10000");
            daValidation.ShowErrorMessage = false;
            daValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            daValidation.ErrorTitle = "listValidationTitle".Translate(lang);
            daValidation.Error = "listValidationMessage".Translate(lang);
            daValidation.Formula.ExcelFormula = $"{_deliveryAddressesSheetName}!A:A";
        }
    }
}
