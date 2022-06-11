using Application.Shared.Addresses;
using Application.Shared.Excel;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Services.Warehouses.Import;
using Domain.Shared;
using Domain.Shared.UserProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Application.Services.Warehouses.Import
{
    public class PoolingWarehousesImportService : IPoolingWarehousesImportService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly ICleanAddressService _addressService;
        private readonly ITriggersService _triggersService;
        private readonly ExcelMapper<PoolingWarehouseDto> _excelMapper;

        public PoolingWarehousesImportService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            ICleanAddressService addressService,
            IFieldDispatcherService fieldDispatcher,
            ITriggersService triggersService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _addressService = addressService;
            _triggersService = triggersService;
            _excelMapper = new ExcelMapper<PoolingWarehouseDto>(dataService, userProvider, fieldDispatcher);
        }

        public Stream GenerateExcelTemplate()
        {
            var excel = new ExcelPackage();
            FillDataSheet(excel);
            return new MemoryStream(excel.GetAsByteArray());
        }

        public OperationDetailedResult ImportFromExcel(Stream fileStream, string fileName)
        {
            var validOrderSyncStatuses = new[] { OrderState.Draft, OrderState.Created, OrderState.Confirmed, OrderState.InShipping };
            var result = new OperationDetailedResult();

            try
            {
                var excel = new ExcelPackage(fileStream);

                var user = _userProvider.GetCurrentUser();
                var lang = user?.Language;

                result.Message = "poolingWarehousesImportTitle".Translate(lang);

                var dataSheet = excel.Workbook.Worksheets.FirstOrDefault();

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

                var warehouseNames = entries.Select(x => x.Data?.WarehouseName)
                                            .Where(x => !string.IsNullOrEmpty(x))
                                            .Distinct()
                                            .ToList();
                var whDbSet = _dataService.GetDbSet<Warehouse>();
                var existsWarehouses = whDbSet.Where(x => x.CompanyId == null && warehouseNames.Contains(x.WarehouseName))
                                              .ToDictionary(x => x.WarehouseName);

                var successNames = new List<string>();
                var errorMessages = new List<string>();
                var emptyNameLineNumbers = new List<string>();
                var emptyDataNames = new List<string>();
                var duplicateLineNumbers = new List<string>();

                var updatedWarehouses = new HashSet<string>();

                foreach (var entry in entries)
                {
                    if (entry.Result.IsError)
                    {
                        errorMessages.Add(entry.Result.Message);
                        continue;
                    }

                    if (entry.Data == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.Data.WarehouseName))
                    {
                        emptyNameLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }

                    if (updatedWarehouses.Contains(entry.Data.WarehouseName))
                    {
                        duplicateLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }
                    updatedWarehouses.Add(entry.Data.WarehouseName);

                    if (string.IsNullOrEmpty(entry.Data.PoolingId)
                        || string.IsNullOrEmpty(entry.Data.ClientPoolingId)
                        || string.IsNullOrEmpty(entry.Data.Client)
                        || string.IsNullOrEmpty(entry.Data.Address))
                    {
                        emptyDataNames.Add(entry.Data.WarehouseName);
                        continue;
                    }

                    bool isNew = false;
                    Warehouse warehouse;
                    if (!existsWarehouses.TryGetValue(entry.Data.WarehouseName, out warehouse))
                    {
                        isNew = true;
                        warehouse = new Warehouse
                        {
                            Id = Guid.NewGuid(),
                            WarehouseName = entry.Data.WarehouseName,
                            DeliveryType = DeliveryType.Delivery,
                            IsActive = true
                        };
                        whDbSet.Add(warehouse);
                    }

                    warehouse.DistributionCenterId = entry.Data.PoolingId;
                    warehouse.PoolingId = entry.Data.ClientPoolingId;
                    warehouse.Client = entry.Data.Client;

                    var address = _addressService.CleanAddress(entry.Data.Address);
                    warehouse.Address = entry.Data.Address;
                    warehouse.Region = string.IsNullOrEmpty(entry.Data.Region) ? address?.Region : entry.Data.Region;
                    warehouse.Area = address?.Area;
                    warehouse.City = address?.City;
                    warehouse.Street = address?.Street;
                    warehouse.House = address?.House;
                    warehouse.PostalCode = address?.PostalCode;
                    warehouse.Latitude = address?.Latitude;
                    warehouse.Longitude = address?.Longitude;
                    warehouse.GeoQuality = address?.GeoQuality;
                    warehouse.ValidAddress = address?.Address;
                    warehouse.UnparsedAddressParts = address?.UnparsedAddressParts;

                    if (!isNew)
                    {
                        var orders = _dataService.GetDbSet<Order>()
                                                 .Where(x => x.DeliveryWarehouseId == warehouse.Id && validOrderSyncStatuses.Contains(x.Status))
                                                 .ToList();

                        foreach (var order in orders)
                        {
                            order.ClientName = warehouse.Client;
                            order.DeliveryRegion = warehouse.Region;
                            order.DeliveryAddress = warehouse.Address;
                        }
                    }

                    successNames.Add(entry.Data.WarehouseName);
                }

                int totalCount = entries.Count();

                AddEntriesGroup(result, lang, totalCount, "poolingWarehousesImportProcessed", successNames, false, 1);
                AddEntriesGroupLineNumbers(result, lang, totalCount, "poolingWarehousesImportEmptyName", emptyNameLineNumbers, true);
                AddEntriesGroup(result, lang, totalCount, "poolingWarehousesImportEmptyData", emptyDataNames, true, 1);
                AddEntriesGroupLineNumbers(result, lang, totalCount, "poolingWarehousesImportDuplicate", duplicateLineNumbers, true);
                AddEntriesGroup(result, lang, totalCount, "poolingWarehousesImportErrorMessages", errorMessages, true, 1);

                var triggerResult = _triggersService.Execute(true);
                if (triggerResult.IsError)
                {
                    result.IsError = true;
                    result.Error = triggerResult.Message;
                    result.Message = null;
                    return result;
                }

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

        private void FillDataSheet(ExcelPackage excel)
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;

            var dataSheet = excel.Workbook.Worksheets.Add("Data");

            _excelMapper.FillSheet(dataSheet, new PoolingWarehouseDto[0], lang);

            int columnsCount = _excelMapper.Columns.Count();
            dataSheet.Cells[1, 1, 1, columnsCount].Style.Font.Bold = true;
            for (int ind = 1; ind <= columnsCount; ind++)
            {
                dataSheet.Column(ind).Width = 20;
            }
        }
    }
}
