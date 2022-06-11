using Application.Shared.Excel;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.FieldProperties;
using Domain.Services.Reports.Registry;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Application.Services.Reports
{
    public class RegistryReportService : IRegistryReportService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IFieldDispatcherService _fieldDispatcherService;

        private const int ColumnsCount = 31;
        private readonly int[] ShippingColumnInds = new[] { 1, 6, 12, 13, 18, 19, 20, 21, 22, 23, 24, 25, 27, 28, 29, 30 };

        public RegistryReportService(ICommonDataService dataService, IUserProvider userProvider, IFieldDispatcherService fieldDispatcherService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _fieldDispatcherService = fieldDispatcherService;
        }

        public Stream ExportReport(RegistryReportParameters filter)
        {
            var excel = new ExcelPackage();

            var sheetName = "registry.Title".Translate(_userProvider.GetCurrentUser()?.Language);
            var workSheet = excel.Workbook.Worksheets.Add(sheetName);

            var entries = GetReportData(filter);

            FillHeaders(workSheet);
            FillData(workSheet, entries);

            return new MemoryStream(excel.GetAsByteArray());
        }

        private List<RegistryEntryDto> GetReportData(RegistryReportParameters filter)
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            DateTime? startDate = filter.StartDate.ToDate();
            DateTime? endDate = filter.EndDate.ToDate()?.AddDays(1);

            var shippingIds = _dataService.GetDbSet<Order>()
                                          .Where(x => x.ShippingDate >= startDate 
                                                    && x.ShippingDate < endDate
                                                    && x.CompanyId == companyId)
                                          .Select(x => x.ShippingId)
                                          .Where(x => x != null)
                                          .Distinct()
                                          .ToList();

            var shippingCarriers = _dataService.GetDbSet<CarrierRequestDatesStat>()
                                               .Include(x => x.Carrier)
                                               .Where(x => shippingIds.Contains(x.ShippingId))
                                               .ToList()
                                               .GroupBy(x => x.ShippingId)
                                               .ToDictionary(x => x.Key, x => x.ToList());

            var orders = _dataService.GetDbSet<Order>()
                                     .Include(x => x.Shipping)
                                     .Include(x => x.BodyType)
                                     .Include(x => x.Carrier)
                                     .Include(x => x.Company)
                                     .Include(x => x.DeliveryWarehouse)
                                     .Include(x => x.VehicleType)
                                     .Include(x => x.VehicleType.Tonnage)
                                     .Where(x => shippingIds.Contains(x.ShippingId))
                                     .ToList();

            var tariffs = _dataService.GetDbSet<Tariff>()
                                      .Include(x => x.Carrier)
                                      .Where(x => x.CompanyId == companyId)
                                      .OrderByDescending(x => x.ExpirationDate)
                                      .ToList();

            var result = new List<RegistryEntryDto>();

            int shippingNumber = 0;
            int shippingDayNumber = 0;
            DateTime? lastShippingDate = null;
            foreach (var shippingOrders in orders.GroupBy(x => x.ShippingId).OrderBy(x => x.Min(y => y.ShippingDate)))
            {
                var shippingDate = shippingOrders.Min(x => x.ShippingDate)?.Date;
                if (shippingDate != lastShippingDate)
                {
                    lastShippingDate = shippingDate;
                    shippingDayNumber = 0;
                }

                var shipping = shippingOrders.First().Shipping;
                var carrier = shipping.Carrier;
                var prevCarrier = carrier;
                if (shippingCarriers.TryGetValue(shippingOrders.Key.Value, out List<CarrierRequestDatesStat> carriers)
                    && carriers.Any(x => x.RejectedAt != null))
                {
                    prevCarrier = carriers.Where(x => x.RejectedAt != null)
                                          .OrderByDescending(x => x.RejectedAt)
                                          .Select(x => x.Carrier)
                                          .FirstOrDefault();
                }

                var prevDeliveryCost = GetPreviousDeliveryCost(shipping, shippingOrders, tariffs);

                ++shippingNumber;
                ++shippingDayNumber;

                foreach (var order in shippingOrders)
                {
                    decimal? orderDeliveryCost = null;
                    if (order.Shipping.WeightKg != null && order.Shipping.WeightKg > 0)
                    {
                        orderDeliveryCost = order.Shipping.BasicDeliveryCostWithoutVAT * order.WeightKg / order.Shipping.WeightKg;
                    }

                    var entry = new RegistryEntryDto
                    {
                        BodyTypeName = order.BodyType?.Name,
                        CarrierName = carrier?.Title,
                        ClientName = order.DeliveryWarehouse?.Client,
                        CompanyName = order.Company?.Name,
                        DeliveryCost = order.Shipping.BasicDeliveryCostWithoutVAT,
                        DeliveryDate = order.DeliveryDate?.Date,
                        DeliveryTime = order.DeliveryDate?.TimeOfDay,
                        DeliveryWarehouseName = order.DeliveryWarehouse?.WarehouseName,
                        DriverName = order.Shipping.DriverName,
                        DriverPhone = order.Shipping.DriverPhone,
                        LoadingDowntimeCost = order.Shipping.LoadingDowntimeCost,
                        Number = shippingNumber,
                        NumberInDay = shippingDayNumber,
                        OldDeliveryCost = prevDeliveryCost,
                        OrderDeliveryCost = orderDeliveryCost,
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber,
                        OrderPalletsCount = order.PalletsCount,
                        OrderVolume = order.Volume,
                        OrderWeight = order.WeightKg / 1000,
                        PlanningCarrierName = prevCarrier?.Title,
                        ReturnCost = order.Shipping.ReturnCostWithoutVAT,
                        ShippingDate = order.ShippingDate?.Date,
                        ShippingId = order.Shipping.Id,
                        ShippingNumber = order.ShippingNumber,
                        ShippingPalletsCount = order.Shipping.PalletsCount,
                        ShippingTime = order.ShippingDate?.TimeOfDay,
                        ShippingWeight = order.Shipping.WeightKg / 1000,
                        Tonnage = order.VehicleType?.Tonnage?.WeightKg / 1000,
                        TenderDeliveryCost = order.Shipping.BasicDeliveryCostWithoutVAT,
                        UnloadingDowntimeCost = order.Shipping.UnloadingDowntimeCost,
                        VehicleNumber = order.Shipping.VehicleNumber
                    };
                    result.Add(entry);
                }
            }
            return result;
        }

        private decimal? GetPreviousDeliveryCost(Shipping shipping, IEnumerable<Order> orders, IEnumerable<Tariff> tariffs)
        {
            decimal? result = null;
            tariffs = tariffs.Where(x => x.CarrierId == shipping.CarrierId && x.TarifficationType == shipping.TarifficationType).ToList();

            foreach (var group in orders.GroupBy(x => new { x.ShippingCity, x.DeliveryCity }))
            {
                var shippingDate = group.Min(x => x.ShippingDate);
                var groupTariffs = tariffs.Where(x => x.ExpirationDate < shippingDate
                                                    && x.ShipmentCity == group.Key.ShippingCity
                                                    && x.DeliveryCity == group.Key.DeliveryCity)
                                          .ToList();

                var tariff = groupTariffs.FirstOrDefault(x => x.BodyTypeId == shipping.BodyTypeId && x.VehicleTypeId == shipping.VehicleTypeId);
                if (tariff == null)
                {
                    tariff = groupTariffs.FirstOrDefault(x => x.BodyTypeId == null && x.VehicleTypeId == null);
                }

                if (tariff == null)
                {
                    continue;
                }

                int totalPallets = (int)Math.Ceiling(orders.Sum(x => x.PalletsCount ?? 0));

                decimal cost;
                if (shipping.TarifficationType == TarifficationType.Ftl 
                    || shipping.TarifficationType == TarifficationType.Doubledeck)
                {
                    cost = tariff.FtlRate ?? 0M;
                }
                else if (shipping.IsPooling == true && tariff.PoolingPalletRate.HasValue)
                {
                    cost = tariff.PoolingPalletRate.Value * totalPallets;
                }
                else
                {
                    cost = GetLtlRate(tariff, totalPallets) ?? 0M;
                }

                bool needWinterCoeff = tariff.StartWinterPeriod != null
                                    && tariff.EndWinterPeriod != null
                                    && shippingDate >= tariff.StartWinterPeriod
                                    && shippingDate <= tariff.EndWinterPeriod
                                    && tariff.WinterAllowance != null;
                if (needWinterCoeff)
                {
                    cost *= 1 + tariff.WinterAllowance.Value / 100;
                }

                result = (result ?? 0M) + cost;
            }

            return result;
        }

        private decimal? GetLtlRate(Tariff tariff, int palletsCount)
        {
            if (palletsCount < 1)
            {
                return 0M;
            }
            else if (palletsCount < 33)
            {
                string propertyName = nameof(tariff.LtlRate33).Replace("33", palletsCount.ToString());
                var property = tariff.GetType().GetProperty(propertyName);
                return (decimal?)property.GetValue(tariff);
            }
            else
            {
                return tariff.LtlRate33;
            }
        }

        private void FillHeaders(ExcelWorksheet worksheet)
        {
            worksheet.Column(1).Width = 5;
            worksheet.Column(2).Width = 8;
            worksheet.Column(3).Width = 8;
            worksheet.Column(4).Width = 8;
            worksheet.Column(5).Width = 16;
            worksheet.Column(6).Width = 10;
            worksheet.Column(7).Width = 12;
            worksheet.Column(8).Width = 8;
            worksheet.Column(9).Width = 14;
            worksheet.Column(10).Width = 30;
            worksheet.Column(11).Width = 30;
            worksheet.Column(12).Width = 8;
            worksheet.Column(13).Width = 8;
            worksheet.Column(14).Width = 12;
            worksheet.Column(15).Width = 10;
            worksheet.Column(16).Width = 10;
            worksheet.Column(17).Width = 10;
            worksheet.Column(18).Width = 16;
            worksheet.Column(19).Width = 16;
            worksheet.Column(20).Width = 16;
            worksheet.Column(21).Width = 14;
            worksheet.Column(22).Width = 14;
            worksheet.Column(23).Width = 16;
            worksheet.Column(24).Width = 30;
            worksheet.Column(25).Width = 16;
            worksheet.Column(26).Width = 20;
            worksheet.Column(27).Width = 18;
            worksheet.Column(28).Width = 22;
            worksheet.Column(29).Width = 22;
            worksheet.Column(30).Width = 22;
            worksheet.Column(31).Width = 16;

            var lang = _userProvider.GetCurrentUser()?.Language;

            SetSingleTitle(worksheet, 1, lang, "registry.Number");
            SetGroupTitles(worksheet, 2, lang, "registry.Order", "registry.OrderPalletsCount",
                                                                 "registry.OrderWeight",
                                                                 "registry.OrderVolume",
                                                                 "registry.OrderNumber"); // 2 - 5
            SetSingleTitle(worksheet, 6, lang, "registry.NumberInDay");
            SetGroupTitles(worksheet, 7, lang, "registry.ShippingArrival", "registry.ShippingDate",
                                                                           "registry.ShippingTime"); // 7 - 8
            SetSingleTitle(worksheet, 9, lang, "registry.CompanyName");
            SetSingleTitle(worksheet, 10, lang, "registry.DeliveryWarehouseName");
            SetSingleTitle(worksheet, 11, lang, "registry.ClientName");
            SetSingleTitle(worksheet, 12, lang, "registry.ShippingWeight");
            SetSingleTitle(worksheet, 13, lang, "registry.ShippingPalletsCount");
            SetSingleTitle(worksheet, 14, lang, "registry.DeliveryDate");
            SetSingleTitle(worksheet, 15, lang, "registry.DeliveryTime");
            SetSingleTitle(worksheet, 16, lang, "registry.BodyTypeName");
            SetSingleTitle(worksheet, 17, lang, "registry.Tonnage");
            SetSingleTitle(worksheet, 18, lang, "registry.TenderDeliveryCost");
            SetSingleTitle(worksheet, 19, lang, "registry.DeliveryCost");
            SetSingleTitle(worksheet, 20, lang, "registry.OldDeliveryCost");
            SetGroupTitles(worksheet, 21, lang, "registry.Carrier", "registry.PlanningCarrierName",
                                                                    "registry.CarrierName"); // 21 - 22
            SetSingleTitle(worksheet, 23, lang, "registry.VehicleNumber");
            SetGroupTitles(worksheet, 24, lang, "registry.Driver", "registry.DriverName",
                                                                   "registry.DriverPhone"); // 24 - 25
            SetSingleTitle(worksheet, 26, lang, "registry.Comments");
            SetSingleTitle(worksheet, 27, lang, "registry.ShippingNumber");
            SetGroupTitles(worksheet, 28, lang, "registry.LoadingDowntimeCost", "registry.Cost");
            SetGroupTitles(worksheet, 29, lang, "registry.UnloadingDowntimeCost", "registry.Cost");
            SetGroupTitles(worksheet, 30, lang, "registry.ReturnCost", "registry.Cost");
            SetSingleTitle(worksheet, 31, lang, "registry.OrderDeliveryCost");

            SetDateStyle(worksheet, 7, 14);
            SetTimeStyle(worksheet, 8, 15);
            SetNumberStyle(worksheet, 3, 4, 12, 17);
            SetCurrencyStyle(worksheet, 18, 19, 20, 28, 29, 30, 31);
        }

        private void SetSingleTitle(ExcelWorksheet worksheet, int colInd, string lang, string titleKey)
        {
            var head = worksheet.Cells[1, colInd, 2, colInd];
            head.Merge = true;
            head.Value = titleKey.Translate(lang);
            SetTitleStyle(head);
        }

        private void SetGroupTitles(ExcelWorksheet worksheet, int startColInd, string lang, string groupTitleKey, params string[] subTitleKeys)
        {
            var head = worksheet.Cells[1, startColInd, 1, startColInd + subTitleKeys.Length - 1];
            head.Merge = true;
            head.Value = groupTitleKey.Translate(lang);
            SetTitleStyle(head);

            for (int i = 0; i < subTitleKeys.Length; i++)
            {
                var subHead = worksheet.Cells[2, startColInd + i];
                subHead.Value = subTitleKeys[i].Translate(lang);
                SetTitleStyle(subHead);
            }
        }

        private void SetTitleStyle(ExcelRange cells)
        {
            cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cells.Style.Font.Bold = true;
            cells.Style.WrapText = true;
            cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        }

        private void SetNumberStyle(ExcelWorksheet worksheet, params int[] colInds)
        {
            SetStyle(worksheet, "0.0", colInds);
        }

        private void SetCurrencyStyle(ExcelWorksheet worksheet, params int[] colInds)
        {
            SetStyle(worksheet, @"# ##0,00 ₽", colInds);
        }

        private void SetDateStyle(ExcelWorksheet worksheet, params int[] colInds)
        {
            SetStyle(worksheet, "dd/MM/yyyy", colInds);
        }

        private void SetTimeStyle(ExcelWorksheet worksheet, params int[] colInds)
        {
            SetStyle(worksheet, "hh:mm;@", colInds);
        }

        private void SetStyle(ExcelWorksheet worksheet, string numberFormat, params int[] colInds)
        {
            foreach (var colInd in colInds)
            {
                worksheet.Column(colInd).Style.Numberformat.Format = numberFormat;
            }
        }

        private void FillData(ExcelWorksheet worksheet, List<RegistryEntryDto> entries)
        {
            var user = _userProvider.GetCurrentUser();

            var excelMapper = new ExcelMapper<RegistryEntryDto>(_dataService, _userProvider, _fieldDispatcherService);
            excelMapper.FillSheetData(worksheet, entries, user?.Language, titleRowsCount: 2);

            Guid? lastShippingId = null;
            for (int i = 0; i < entries.Count; i++)
            {
                int rowInd = i + 3;
                bool isFirstRow = entries[i].ShippingId != lastShippingId;
                if (isFirstRow)
                {
                    worksheet.Cells[rowInd, 1, rowInd, ColumnsCount].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                }
                else
                {
                    foreach (var colInd in ShippingColumnInds)
                    {
                        worksheet.Cells[rowInd, colInd].Value = null;
                    }
                }

                int lastRowInd = entries.Count + 2;
                worksheet.Cells[lastRowInd, 1, lastRowInd, ColumnsCount].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                lastShippingId = entries[i].ShippingId;
            }
        }
    }
}
