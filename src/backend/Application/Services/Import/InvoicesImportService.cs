using Application.Shared.Excel;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.History;
using Domain.Services.Import;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Application.Services.Import
{
    public class InvoicesImportService : IInvoicesImportService
    {
        private const string _dictionarySheetName = "dictionary";
        private const string _dataSheetName = "Data";

        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IShippingCalculationService _shippingCalculationService;
        private readonly IChangeTrackerFactory _changeTrackerFactory;
        private readonly IHistoryService _historyService;


        private readonly ExcelMapper<InvoicesImportDto> _excelMapper;

        public InvoicesImportService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IFieldDispatcherService fieldDispatcher,
            IShippingCalculationService shippingCalculationService,
            IChangeTrackerFactory changeTrackerFactory,
            IHistoryService historyService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _shippingCalculationService = shippingCalculationService;
            _changeTrackerFactory = changeTrackerFactory;
            _historyService = historyService;

            _excelMapper = new ExcelMapper<InvoicesImportDto>(dataService, userProvider, fieldDispatcher);
        }

        public byte[] GenerateExcelTemplate()
        {
            var excel = new ExcelPackage();

            FillDictionarySheet(excel);
            FillDataSheet(excel);

            return excel.GetAsByteArray();
        }

        private void FillDictionarySheet(ExcelPackage excel)
        {
            var dictSheet = excel.Workbook.Worksheets.Add(_dictionarySheetName);
            dictSheet.Hidden = eWorkSheetHidden.VeryHidden;

            dictSheet.Cells[1, 1].Value = "Да";
            dictSheet.Cells[2, 1].Value = "Нет";
        }

        private void FillDataSheet(ExcelPackage excel)
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;

            var dataSheet = excel.Workbook.Worksheets.Add(_dataSheetName);

            _excelMapper.FillSheet(dataSheet, new InvoicesImportDto[0], lang);

            int columnsCount = _excelMapper.Columns.Count();
            dataSheet.Cells[1, 1, 1, columnsCount].Style.Font.Bold = true;
            for (int ind = 1; ind <= columnsCount; ind++)
            {
                dataSheet.Column(ind).Width = 20;
            }

            var validation = dataSheet.DataValidations.AddListValidation("G1:G10000");
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = "listValidationTitle".Translate(lang);
            validation.Error = "listValidationMessage".Translate(lang);
            validation.Formula.ExcelFormula = $"{_dictionarySheetName}!A:A";
        }

        public OperationDetailedResult ImportFromExcel(Stream fileStream, string fileName)
        {
            var result = new OperationDetailedResult();

            try
            {
                var excel = new ExcelPackage(fileStream);

                var user = _userProvider.GetCurrentUser();
                var lang = user?.Language;

                var dataSheet = excel.Workbook.Worksheets[_dataSheetName] ?? excel.Workbook.Worksheets.FirstOrDefault();
                var entries = _excelMapper.LoadEntries(dataSheet, lang);

                var errorMessages = new List<string>();
                var emptyNumberLineNumbers = new List<string>();
                var duplicatOrderLineNumbers = new List<string>();
                var orderNotFoundNumbers = new List<string>();
                var shippingNotFoundNumbers = new List<string>();

                var updatedOrderNumbers = new HashSet<string>();
                var updatedShippings = new Dictionary<string, List<string>>();

                foreach (var entry in entries)
                {
                    if (entry.Data == null)
                    {
                        continue;
                    }

                    if (entry.Result.IsError)
                    {
                        errorMessages.Add("invoicesImportError".Translate(lang, entry.RecordNumber, entry.Result.Message));
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.Data.OrderNumber))
                    {
                        emptyNumberLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }

                    if (updatedOrderNumbers.Contains(entry.Data.OrderNumber))
                    {
                        duplicatOrderLineNumbers.Add(entry.RecordNumber.ToString());
                        continue;
                    }

                    var order = _dataService.GetAll<Order>(i => i.OrderNumber == entry.Data.OrderNumber).FirstOrDefault();

                    if (order == null)
                    {
                        orderNotFoundNumbers.Add(entry.Data.OrderNumber);
                        continue;
                    }

                    var shipping = order.ShippingId.HasValue ? _dataService.GetById<Shipping>(order.ShippingId.Value) : null;

                    if (shipping == null)
                    {
                        shippingNotFoundNumbers.Add(entry.Data.OrderNumber);
                        continue;
                    }

                    if (!updatedShippings.ContainsKey(shipping.ShippingNumber))
                    {
                        shipping.DeliveryInvoiceNumber = entry.Data.DeliveryAccountNumber;
                        shipping.ActualTotalDeliveryCostWithoutVAT = entry.Data.ActualTotalDeliveryCostWithoutVAT;
                        shipping.OtherCosts = entry.Data.OtherExpenses;

                        shipping.TrucksDowntime = entry.Data.TrucksDowntime;
                        shipping.DowntimeRate = entry.Data.DowntimeAmount;

                        _historyService.Save(shipping.Id, "InvoicesImportUpdatedHistory", fileName);
                    }

                    order.IsReturn = entry.Data.Return;
                    order.ReturnShippingCost = entry.Data.ReturnShippingCost;


                    var shippingOrders = _dataService.GetAll<Order>(i => i.ShippingId == shipping.Id && i.Id != order.Id).ToList()
                        .Union(new[] { order });

                    shippingOrders.ToList().ForEach(i =>
                    {
                        i.DeliveryAccountNumber = entry.Data.DeliveryAccountNumber;
                        i.TrucksDowntime = shipping.TrucksDowntime * i.PalletsCount / shipping.PalletsCount;
                        i.DowntimeAmount = shipping.DowntimeRate * i.PalletsCount / shipping.PalletsCount;
                    });

                    _shippingCalculationService.RecalculateDeliveryCosts(shipping, shippingOrders);
                    _shippingCalculationService.RecalculateShippingOrdersCosts(shipping, shippingOrders);

                    shippingOrders.ToList().ForEach(i => _historyService.Save(i.Id, "InvoicesImportUpdatedHistory", fileName));

                    updatedOrderNumbers.Add(entry.Data.OrderNumber);

                    if (!updatedShippings.ContainsKey(shipping.ShippingNumber))
                    {
                        updatedShippings[shipping.ShippingNumber] = new List<string>();
                    }

                    updatedShippings[shipping.ShippingNumber].Add(order.OrderNumber);
                }

                int totalCount = entries.Count();

                result.Message = "invoicesImportTitle".Translate(lang);

                var shippingMessage = "shipping".Translate(lang);
                var successMessage = updatedShippings.Select(i => $"{shippingMessage} {i.Key}: {string.Join(", ", i.Value)}").ToList();

                AddEntriesGroup(result, lang, totalCount, "invoicesImportProcessed", successMessage, false, 1);
                AddEntriesGroup(result, lang, totalCount, "invoicesImportOrderNotFound", orderNotFoundNumbers, true, 3);
                AddEntriesGroup(result, lang, totalCount, "invoicesImportShippingNotFound", shippingNotFoundNumbers, true, 3);

                AddEntriesGroupLineNumbers(result, lang, totalCount, "invoicesImportShippingDuplicated", duplicatOrderLineNumbers, true);
                AddEntriesGroupLineNumbers(result, lang, totalCount, "invoicesImportEmptyData", emptyNumberLineNumbers, true);
                AddEntriesGroup(result, lang, totalCount, "invoicesImportErrorMessages", errorMessages, true, 1);

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

        //TODO: вынести в хэлпер

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
                string message = "shippingVehicleLinesList".Translate(lang, lineNumbers);
                AddEntriesGroup(result, lang, totalCount, titleKey, new List<string> { message }, isError, 1, messages.Count);
            }
        }
    }
}