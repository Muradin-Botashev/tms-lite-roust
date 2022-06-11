using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Translations;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.TransportCompanies
{
    public class CarrierSelectionService : ICarrierSelectionService
    {
        private readonly ICommonDataService _dataService;
        private readonly ISendShippingService _sendShippingService;
        private readonly IShippingCalculationService _shippingCalculationService;
        private readonly IDeliveryCostCalcService _deliveryCostCalcService;

        public CarrierSelectionService(
            ICommonDataService dataService,
            ISendShippingService sendShippingService,
            IShippingCalculationService shippingCalculationService,
            IDeliveryCostCalcService deliveryCostCalcService)
        {
            _dataService = dataService;
            _sendShippingService = sendShippingService;
            _shippingCalculationService = shippingCalculationService;
            _deliveryCostCalcService = deliveryCostCalcService;
        }

        public Guid? FindCarrier(Shipping shipping, List<Order> orders, out Tariff tariff, out CarrierSelectionType type, params Guid?[] ignoredCarrierIds)
        {
            using (LogContext.PushProperty("Type", "CarrierSelection"))
            {
                tariff = null;
                type = CarrierSelectionType.None;

                // Пропускаем неполные перевозки
                if (shipping == null || shipping.ShippingDate == null)
                {
                    return null;
                }

                // Если не переданы в метод накладные, грузим сами
                if (orders == null)
                {
                    orders = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId == shipping.Id)
                                         .ToList();
                }

                // Игнорируются ТК, которые уже участвовали в перевозке
                var allIgnoredCarrierIds = _dataService.GetDbSet<CarrierShippingAction>()
                                                       .Where(x => x.ShippingId == shipping.Id)
                                                       .Select(x => x.CarrierId)
                                                       .ToList();
                if (ignoredCarrierIds != null)
                {
                    allIgnoredCarrierIds.AddRange(ignoredCarrierIds);
                }

                // Берем оптимальную из закрепленных ТК, если есть
                var fixedDirections = FindFixedDirections(shipping, orders, allIgnoredCarrierIds);
                var fixedDirection = fixedDirections.Any() ? ChooseFixedDirection(shipping, fixedDirections) : null;
                if (fixedDirection == null)
                {
                    Log.Information("Подбор ТК. Не найдено закрепленных ТК.");
                }
                else
                {
                    Log.Information($"Подбор ТК. ТК подобрана из числа закрепленных: {fixedDirection.Carrier?.Title}");
                }

                var result = fixedDirection?.CarrierId;
                var nonNullIgnoredCarrierIds = allIgnoredCarrierIds.Where(x => x != null).Select(x => x.Value).ToList();
                tariff = _deliveryCostCalcService.FindTariff(shipping, orders, fixedDirection?.CarrierId, fixedDirection?.VehicleTypeIds, null, nonNullIgnoredCarrierIds);

                if (result == null)
                {
                    // Система выполняет проверку на график отгрузок Алгоритм проверки графика отгрузок 
                    if (tariff != null && tariff.TarifficationType != TarifficationType.Ftl)
                    {
                        var hasScheduleEntry = CheckShippingSchedule(tariff, orders);
                        if (hasScheduleEntry)
                        {
                            Log.Information("Подбор ТК. Не-FTL перевозка соответствует графику огрузок");
                        }
                        else
                        {
                            Log.Warning("Подбор ТК. Не-FTL перевозка НЕ соответствует графику огрузок, ищется FTL вариант тарифа.");
                            tariff = _deliveryCostCalcService.FindTariff(shipping, orders, fixedDirection?.CarrierId, fixedDirection?.VehicleTypeIds, TarifficationType.Ftl, nonNullIgnoredCarrierIds);
                        }
                    }

                    // Связь транспортной компании и перевозки должна отсутствовать в таблице связей Таблица связей перевозок и перевозчиков
                    if (tariff != null && allIgnoredCarrierIds.Contains(tariff.CarrierId))
                    {
                        Log.Warning("Подбор ТК. Выбранная по тарифу ТК находится в списке игнорируемых.");
                        tariff = null;
                    }

                    result = tariff?.CarrierId;
                    if (result == null)
                    {
                        Log.Warning("Подбор ТК. Не найдено подходящих ТК по тарифам.");
                    }   
                    else
                    {
                        Log.Information($"Подбор ТК. ТК подобрана по оптимальному тарифу: {tariff.Carrier?.Title}");
                        type = CarrierSelectionType.BestCost;
                    }
                }
                else
                {
                    type = CarrierSelectionType.FixedDirection;

                    if (tariff == null && fixedDirection.VehicleTypeIds != null && fixedDirection.VehicleTypeIds.Any())
                    {
                        var vehicleTypeIdsList = fixedDirection.VehicleTypeIds.ToList();
                        var optimalVehicleType = _dataService.GetDbSet<VehicleType>()
                                                             .Include(x => x.Tonnage)
                                                             .Where(x => vehicleTypeIdsList.Contains(x.Id)
                                                                        && x.PalletsCount >= shipping.PalletsCount
                                                                        && x.Tonnage.WeightKg >= shipping.WeightKg)
                                                             .OrderBy(x => x.PalletsCount)
                                                             .FirstOrDefault();
                        if (optimalVehicleType == null)
                        {
                            Log.Warning("Подбор ТК. Не найдено подходящих типов ТС.");
                        }
                        else
                        {
                            Log.Information($"Подбор ТК. Смена Типа ТС на оптимальный из закрепления: {optimalVehicleType.Name}.");
                            shipping.VehicleTypeId = optimalVehicleType.Id;
                        }
                    }
                }

                return result;
            }
        }

        public void UpdateCarrier(Shipping shipping, List<Order> orders, Guid carrierId, Tariff tariff = null)
        {
            if (shipping == null)
            {
                return;
            }

            if (orders == null)
            {
                orders = _dataService.GetDbSet<Order>()
                                     .Where(x => x.ShippingId == shipping.Id)
                                     .ToList();
            }

            var requestsDbSet = _dataService.GetDbSet<CarrierRequestDatesStat>();

            if (shipping.CarrierId != null)
            {
                var rejectedRequestEntry = requestsDbSet.FirstOrDefault(x => x.ShippingId == shipping.Id && x.CarrierId == shipping.CarrierId);
                if (rejectedRequestEntry == null)
                {
                    rejectedRequestEntry = new CarrierRequestDatesStat
                    {
                        Id = Guid.NewGuid(),
                        ShippingId = shipping.Id,
                        CarrierId = shipping.CarrierId.Value,
                        SentAt = DateTime.Now
                    };
                    requestsDbSet.Add(rejectedRequestEntry);
                }
                rejectedRequestEntry.RejectedAt = DateTime.Now;
            }

            shipping.CarrierId = carrierId;
            shipping.TarifficationType = tariff?.TarifficationType ?? shipping.TarifficationType;
            shipping.BodyTypeId = shipping.BodyTypeId ?? tariff?.BodyTypeId;
            shipping.VehicleTypeId = shipping.VehicleTypeId ?? tariff?.VehicleTypeId;

            foreach (var order in orders)
            {
                order.CarrierId = shipping.CarrierId;
                order.TarifficationType = shipping.TarifficationType;
                order.BodyTypeId = shipping.BodyTypeId;
                order.VehicleTypeId = shipping.VehicleTypeId;
            }

            var requestEntry = requestsDbSet.FirstOrDefault(x => x.ShippingId == shipping.Id && x.CarrierId == shipping.CarrierId);
            if (requestEntry == null)
            {
                requestEntry = new CarrierRequestDatesStat
                {
                    Id = Guid.NewGuid(),
                    ShippingId = shipping.Id,
                    CarrierId = shipping.CarrierId.Value
                };
                requestsDbSet.Add(requestEntry);
            }
            requestEntry.SentAt = DateTime.Now;
            requestEntry.RejectedAt = null;
            requestEntry.ConfirmedAt = null;

            _deliveryCostCalcService.UpdateDeliveryCost(tariff, shipping, orders);
            _shippingCalculationService.RecalculateDeliveryCosts(shipping, orders);

            _sendShippingService.SendShippingToTk(shipping, orders);
        }

        public void FindAndUpdateCarrier(Shipping shipping, List<Order> orders, params Guid?[] ignoredCarrierIds)
        {
            if (orders == null)
            {
                orders = _dataService.GetDbSet<Order>()
                                     .Where(x => x.ShippingId == shipping.Id)
                                     .ToList();
            }

            var carrierId = FindCarrier(shipping, orders, out Tariff tariff, out CarrierSelectionType _, ignoredCarrierIds);
            if (carrierId != null)
            {
                UpdateCarrier(shipping, orders, carrierId.Value, tariff);
            }
        }

        private List<FixedDirection> FindFixedDirections(Shipping shipping, List<Order> orders, List<Guid?> ignoredCarrierIds)
        {
            // Первая и последняя накладная нужны для получения городов и регионов перевозки
            var firstOrder = orders.OrderBy(x => x.ShippingDate).FirstOrDefault();
            var lastOrder = orders.OrderByDescending(x => x.DeliveryDate).FirstOrDefault();

            var validVehicleTypeIds = _dataService.GetDbSet<VehicleType>()
                                                  .Include(x => x.Tonnage)
                                                  .Where(x => shipping.PalletsCount <= x.PalletsCount
                                                            && shipping.WeightKg <= x.Tonnage.WeightKg)
                                                  .Select(x => x.Id)
                                                  .ToList();

            // Получаем из базы все закрепленные направления, которые удовлетворяют всем условиям, кроме направления
            var commonFixedDirections = _dataService.GetDbSet<FixedDirection>()
                                                    .Include(x => x.Carrier)
                                                    .Where(x => (x.VehicleTypeIds == null 
                                                                    || x.VehicleTypeIds.Length == 0 
                                                                    || x.VehicleTypeIds.Any(i => validVehicleTypeIds.Contains(i)))
                                                                && !ignoredCarrierIds.Contains(x.CarrierId)
                                                                && x.IsActive)
                                                    .ToList();

            // Проверяем наличие подходящих направлений по складам
            var fixedDirections = commonFixedDirections.Where(x => x.ShippingWarehouseId == shipping.ShippingWarehouseId
                                                                && x.DeliveryWarehouseId == shipping.DeliveryWarehouseId)
                                                       .ToList();

            // Иначе по городам
            if (!fixedDirections.Any())
            {
                fixedDirections = commonFixedDirections.Where(x => x.ShippingCity == firstOrder.ShippingCity
                                                                && x.DeliveryCity == lastOrder.DeliveryCity)
                                                       .ToList();
            }

            // Иначе по регионам
            if (!fixedDirections.Any())
            {
                fixedDirections = commonFixedDirections.Where(x => x.ShippingRegion == firstOrder.ShippingRegion
                                                                && x.DeliveryRegion == lastOrder.DeliveryRegion)
                                                       .ToList();
            }

            return fixedDirections;
        }

        private bool CheckShippingSchedule(Tariff tariff, IEnumerable<Order> orders)
        {
            var firstOrder = orders.OrderBy(x => x.ShippingDate).FirstOrDefault();
            var lastOrder = orders.OrderByDescending(x => x.DeliveryDate).FirstOrDefault();

            var shippingDay = GetWeekDay(firstOrder.ShippingDate);
            var deliveryDay = GetWeekDay(lastOrder.DeliveryDate);

            var shippingDayName = ((WeekDay)shippingDay).FormatEnum().Translate("ru");
            var deliveryDayName = ((WeekDay)deliveryDay).FormatEnum().Translate("ru");

            Log.Information($"Подбор ТК. Проверка графика по параметрам: {tariff.Carrier?.Title}, {firstOrder.ShippingCity} ({shippingDayName}) - {lastOrder.DeliveryCity} ({deliveryDayName}).");

            var scheduleEntry = _dataService.GetDbSet<ShippingSchedule>()
                                            .Where(x => x.CarrierId == tariff.CarrierId
                                                        && x.ShippingCity == firstOrder.ShippingCity
                                                        && x.DeliveryCity == lastOrder.DeliveryCity
                                                        && x.ShippingDays != null
                                                        && x.DeliveryDays != null)
                                            .ToList()
                                            .Where(x => x.ShippingDays.ToList().Contains(shippingDay)
                                                        && x.DeliveryDays.ToList().Contains(deliveryDay))
                                            .FirstOrDefault();

            return scheduleEntry != null;
        }

        private int GetWeekDay(DateTime? date)
        {
            if (date == null)
            {
                return default;
            }
            int value = (int)date.Value.DayOfWeek;
            if (value == 0)
            {
                value = 7;
            }
            return value;
        }

        private FixedDirection ChooseFixedDirection(Shipping shipping, List<FixedDirection> fixedDirections)
        {
            if (fixedDirections.Count > 1)
            {
                // Расчет использованной квоты по ТК в том же месяце, что и Дата отгрузки превозки
                var shippingDate = shipping.ShippingDate.Value;
                var monthStart = new DateTime(shippingDate.Year, shippingDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var allMonthCarriers = _dataService.GetDbSet<Shipping>()
                                                   .Where(x => x.ShippingDate >= monthStart && x.ShippingDate < monthEnd && x.Id != shipping.Id && x.CarrierId != null)
                                                   .Select(x => x.CarrierId)
                                                   .ToList();

                var monthCarriers = allMonthCarriers.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
                var monthCarriersCount = allMonthCarriers.Count + 1;

                var carrierLogData = fixedDirections.Select(x => $"{x.Carrier.Title} ({GetPercentage(monthCarriers, monthCarriersCount, x.CarrierId)} / {x.Quota} %)");
                Log.Information($"Подбор ТК. Найдены закрепленные ТК (квота в случае выбора / квота направления): {string.Join(", ", carrierLogData)}.");

                // Выбирается вариант с наибольшим запасом по квоте
                return fixedDirections.OrderByDescending(x => x.Quota - GetPercentage(monthCarriers, monthCarriersCount, x.CarrierId)).First();
            }
            else
            {
                var result = fixedDirections.First();
                Log.Information($"Подбор ТК. Найдена единственная закрепленная ТК: {result.Carrier.Title}.");
                return result;
            }
        }

        private decimal GetPercentage(Dictionary<Guid?, int> counts, int totalCount, Guid? id)
        {
            if (counts.TryGetValue(id, out int result))
            {
                return (result + 1M) * 100M / totalCount;
            }
            else
            {
                return 100M / totalCount;
            }
        }
    }
}
