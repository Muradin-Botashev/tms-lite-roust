using Application.Shared.Excel;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.History;
using Domain.Services.Shippings.Import;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Services.Shippings.Import
{
    public class ShippingVehicleImportService : IShippingVehicleImportService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IHistoryService _historyService;
        private readonly IChangeTrackerFactory _changeTrackerFactory;
        private readonly ExcelMapper<ShippingVehicleImportDto> _excelMapper;

        private const string _dataSheetName = "Data";
        private const string _vehicleTypesSheetName = "VehicleTypes";

        public ShippingVehicleImportService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IHistoryService historyService,
            IChangeTrackerFactory changeTrackerFactory,
            IFieldDispatcherService fieldDispatcher)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _historyService = historyService;
            _changeTrackerFactory = changeTrackerFactory;
            _excelMapper = new ExcelMapper<ShippingVehicleImportDto>(dataService, userProvider, fieldDispatcher);
        }

        public Stream GenerateExcelTemplate()
        {
            var excel = new ExcelPackage();

            FillVehicleTypes(excel);
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

                result.Message = "shippingVehicleTitle".Translate(lang);

                var dataSheet = excel.Workbook.Worksheets[_dataSheetName];

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

                Dictionary<string, Shipping> shippingsDict = LoadShippings(entries);
                Dictionary<string, VehicleType> vehicleTypesDict = LoadVehicleTypes();

                var successNumbers = new List<string>();
                var errorMessages = new List<string>();
                var emptyNumberLineNumbers = new List<string>();
                var duplicateLineNumbers = new List<string>();
                var emptyDataNumbers = new List<string>();
                var notFoundNumbers = new List<string>();
                var wrongStatusNumbers = new List<string>();

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

                    if (string.IsNullOrEmpty(entry.Data.ShippingNumber))
                    {
                        emptyNumberLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }

                    if (!shippingsDict.TryGetValue(entry.Data.ShippingNumber, out Shipping shipping))
                    {
                        notFoundNumbers.Add(entry.Data.ShippingNumber);
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.Data.DriverName)
                        && string.IsNullOrEmpty(entry.Data.DriverPassportData)
                        && string.IsNullOrEmpty(entry.Data.DriverPhone)
                        && string.IsNullOrEmpty(entry.Data.VehicleNumber)
                        && string.IsNullOrEmpty(entry.Data.TrailerNumber)
                        && string.IsNullOrEmpty(entry.Data.VehicleMake)
                        && string.IsNullOrEmpty(entry.Data.VehicleType))
                    {
                        emptyDataNumbers.Add(entry.Data.ShippingNumber);
                        continue;
                    }

                    if (updatedShippings.Contains(entry.Data.ShippingNumber))
                    {
                        duplicateLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }
                    updatedShippings.Add(entry.Data.ShippingNumber);

                    if (shipping.Status != ShippingState.ShippingRequestSent
                        && shipping.Status != ShippingState.ShippingConfirmed
                        && shipping.Status != ShippingState.ShippingSlotBooked
                        && shipping.Status != ShippingState.ShippingChangesAgreeing)
                    {
                        wrongStatusNumbers.Add(entry.Data.ShippingNumber);
                        continue;
                    }

                    shipping.DriverName = GetFieldValue(entry.Data, x => x.DriverName, shipping.DriverName);
                    shipping.DriverPassportData = GetFieldValue(entry.Data, x => x.DriverPassportData, shipping.DriverPassportData);
                    shipping.DriverPhone = GetFieldValue(entry.Data, x => x.DriverPhone, shipping.DriverPhone);
                    shipping.VehicleNumber = GetFieldValue(entry.Data, x => x.VehicleNumber, shipping.VehicleNumber);
                    shipping.TrailerNumber = GetFieldValue(entry.Data, x => x.TrailerNumber, shipping.TrailerNumber);
                    shipping.VehicleMake = GetFieldValue(entry.Data, x => x.VehicleMake, shipping.VehicleMake);

                    if (!string.IsNullOrEmpty(entry.Data.VehicleType))
                    {
                        if (vehicleTypesDict.TryGetValue(entry.Data.VehicleType, out VehicleType vehicleType))
                        {
                            shipping.VehicleTypeId = vehicleType.Id;
                            shipping.BodyTypeId = vehicleType.BodyTypeId;
                        }
                        else
                        {
                            entry.Result.AddError(nameof(entry.Data.VehicleType),
                                "EntityNotFound".Translate(lang, nameof(entry.Data.VehicleType)),
                                ValidationErrorType.Exception);
                        }
                    }

                    var orders = _dataService.GetAll<Order>(i => i.ShippingId == shipping.Id).ToList();

                    orders.ForEach(i =>
                    {
                        i.DriverName = shipping.DriverName;
                        i.DriverPassportData = shipping.DriverPassportData;
                        i.DriverPhone = shipping.DriverPhone;
                        i.VehicleNumber = shipping.VehicleNumber;
                        i.TrailerNumber = shipping.TrailerNumber;
                        i.VehicleMake = shipping.VehicleMake;
                        i.VehicleTypeId = shipping.VehicleTypeId;
                        i.BodyTypeId = shipping.BodyTypeId;
                    });

                    _historyService.Save(shipping.Id, "shippingVehicleUpdatedFromFile", fileName);

                    successNumbers.Add(entry.Data.ShippingNumber);
                }

                int totalCount = entries.Count();

                AddEntriesGroup(result, lang, totalCount, "shippingVehicleShippingProcessed", successNumbers, false, 4);
                AddEntriesGroup(result, lang, totalCount, "shippingVehicleEmptyData", emptyDataNumbers, true, 4);
                AddEntriesGroup(result, lang, totalCount, "shippingVehicleWrongShippingStatus", wrongStatusNumbers, true, 4);
                AddEntriesGroup(result, lang, totalCount, "shippingVehicleShippingNotFound", notFoundNumbers, true, 4);
                AddEntriesGroupLineNumbers(result, lang, totalCount, "shippingVehicleShippingDuplicate", duplicateLineNumbers, true);
                AddEntriesGroupLineNumbers(result, lang, totalCount, "shippingVehicleEmptyNumber", emptyNumberLineNumbers, true);
                AddEntriesGroup(result, lang, totalCount, "shippingVehicleErrorMessages", errorMessages, true, 1);

                _changeTrackerFactory.CreateChangeTracker()
                                     .TrackAll<Shipping>()
                                     .TrackAll<Order>()
                                     .LogTrackedChanges();

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

        private string GetFieldValue(ShippingVehicleImportDto dto, Expression<Func<ShippingVehicleImportDto, string>> property, string defaultValue)
        {
            string newValue = property.Compile()(dto);

            if (!string.IsNullOrEmpty(newValue))
            {
                var propertyBody = property?.Body as MemberExpression;
                if (propertyBody != null)
                {
                    var maxLenAttr = propertyBody.Member.GetCustomAttribute<MaxLengthAttribute>();
                    if (maxLenAttr != null)
                    {
                        newValue = newValue.Substring(0, Math.Min(newValue.Length, maxLenAttr.Length));
                    }
                }
                return newValue;
            }
            else
            {
                return defaultValue;
            }
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

        private Dictionary<string, Shipping> LoadShippings(IEnumerable<ValidatedRecord<ShippingVehicleImportDto>> entries)
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;

            var shippingNumbers = entries.Select(x => x.Data?.ShippingNumber)
                                         .Where(x => !string.IsNullOrEmpty(x))
                                         .Distinct()
                                         .ToList();

            var shippings = _dataService.GetDbSet<Shipping>()
                                        .Where(x => shippingNumbers.Contains(x.ShippingNumber)
                                                && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                        .ToList();

            var result = new Dictionary<string, Shipping>();
            foreach (var shipping in shippings)
            {
                result[shipping.ShippingNumber] = shipping;
            }

            return result;
        }

        private Dictionary<string, VehicleType> LoadVehicleTypes()
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            var vehicleTypes = _dataService.GetDbSet<VehicleType>()
                                           .Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                           .ToList();

            var result = new Dictionary<string, VehicleType>();
            foreach (var shipping in vehicleTypes)
            {
                result[shipping.Name] = shipping;
            }

            return result;
        }

        private void FillVehicleTypes(ExcelPackage excel)
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            var vehicleTypesSheet = excel.Workbook.Worksheets.Add(_vehicleTypesSheetName);
            vehicleTypesSheet.Hidden = eWorkSheetHidden.VeryHidden;

            var vehicleTypes = _dataService.GetDbSet<VehicleType>()
                                           .Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                           .Select(x => x.Name)
                                           .OrderBy(x => x)
                                           .ToList();

            for (int ind = 0; ind < vehicleTypes.Count; ind++)
            {
                vehicleTypesSheet.Cells[ind + 1, 1].Value = vehicleTypes[ind];
            }
        }

        private void FillDataSheet(ExcelPackage excel)
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;

            var dataSheet = excel.Workbook.Worksheets.Add(_dataSheetName);

            _excelMapper.FillSheet(dataSheet, new ShippingVehicleImportDto[0], lang);

            int columnsCount = _excelMapper.Columns.Count();
            dataSheet.Cells[1, 1, 1, columnsCount].Style.Font.Bold = true;
            for (int ind = 1; ind <= columnsCount; ind++)
            {
                dataSheet.Column(ind).Width = 20;
            }

            var validation = dataSheet.DataValidations.AddListValidation("H2:H10000");
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = "listValidationTitle".Translate(lang);
            validation.Error = "listValidationMessage".Translate(lang);
            validation.Formula.ExcelFormula = $"{_vehicleTypesSheetName}!A:A";
        }
    }
}
