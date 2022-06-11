using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Services.Translations;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.Shippings
{
    public class DeliveryCostCalcService : IDeliveryCostCalcService
    {
        private readonly ICommonDataService _commonDataService;
        private readonly IHistoryService _historyService;

        public DeliveryCostCalcService(
            ICommonDataService commonDataService,
            IHistoryService historyService)
        {
            _commonDataService = commonDataService;
            _historyService = historyService;
        }

        public void UpdateDeliveryCost(Shipping shipping, IEnumerable<Order> orders = null, bool ignoreManualCost = false)
        {
            var validState = new[] {
                ShippingState.ShippingCreated,
                ShippingState.ShippingRequestSent,
                ShippingState.ShippingRejectedByTc,
                ShippingState.ShippingSlotBooked,
                ShippingState.ShippingSlotCancelled,
                ShippingState.ShippingChangesAgreeing
            };
            if (shipping.Status == null
                || shipping.DeliveryType != DeliveryType.Delivery
                || !validState.Contains(shipping.Status.Value)
                || shipping.CarrierId == null
                || shipping.TarifficationType == null)
            {
                return;
            }

            if (orders == null)
            {
                orders = _commonDataService.GetDbSet<Order>()
                                           .Where(x => x.ShippingId == shipping.Id)
                                           .ToList();
            }

            var hasIncompleteOrders = orders.Where(x => (!ignoreManualCost && x.ManualDeliveryCost)
                                                        || x.ShippingDate == null
                                                        || x.DeliveryDate == null)
                                            .Any();
            if (hasIncompleteOrders || !orders.Any())
            {
                return;
            }

            Log.Information("Расчет стоимости перевозки запущен для {ShippingNumber}", shipping.ShippingNumber);

            var tariff = FindTariff(shipping, orders, shipping.CarrierId);
            UpdateDeliveryCost(tariff, shipping, orders);
        }

        public Tariff FindTariff(
            Shipping shipping, 
            IEnumerable<Order> orders = null, 
            Guid? carrierId = null, 
            IEnumerable<Guid> vehicleTypeIds = null, 
            TarifficationType? tarifficationType = null,
            IEnumerable<Guid> ignoredCarrierIds = null)
        {
            if (shipping == null)
            {
                return null;
            }

            if (orders == null)
            {
                orders = _commonDataService.GetDbSet<Order>().Where(x => x.ShippingId == shipping.Id).ToList();
            }

            var vehicleTypesDict = _commonDataService.GetDbSet<VehicleType>().ToDictionary(x => x.Id);
            var carriersDict = _commonDataService.GetDbSet<TransportCompany>().ToDictionary(x => x.Id);

            var firstOrder = orders.OrderBy(x => x.ShippingDate).FirstOrDefault();
            var lastOrder = orders.OrderByDescending(x => x.DeliveryDate).FirstOrDefault();
            var shippingDate = firstOrder?.ShippingDate?.Date;

            var shippingWarehouse = firstOrder.ShippingWarehouseId == null ? null : _commonDataService.GetById<ShippingWarehouse>(firstOrder.ShippingWarehouseId.Value);
            var deliveryWarehouse = lastOrder.DeliveryWarehouseId == null ? null : _commonDataService.GetById<Warehouse>(lastOrder.DeliveryWarehouseId.Value);

            var findParams = new List<string>
            {
                shippingDate.FormatDate(),
                $"{shippingWarehouse?.WarehouseName ?? "(нет склада)"} - {deliveryWarehouse?.WarehouseName ?? "(нет склада)"}",
                $"{firstOrder.ShippingCity} - {lastOrder.DeliveryCity}",
                $"{firstOrder.ShippingRegion} - {lastOrder.DeliveryRegion}",
                $"{shipping.PalletsCount} паллет",
                $"{shipping.WeightKg} кг"
            };

            var query = _commonDataService.GetDbSet<Tariff>()
                            .Include(x => x.Carrier)
                            .Include(x => x.VehicleType)
                            .Include(x => x.VehicleType.Tonnage)
                            .Where(x => x.EffectiveDate <= shippingDate && shippingDate <= x.ExpirationDate)
                            .Where(x => x.TarifficationType != TarifficationType.Ftl
                                    || (x.VehicleType != null 
                                        && shipping.PalletsCount <= x.VehicleType.PalletsCount
                                        && shipping.WeightKg <= x.VehicleType.Tonnage.WeightKg));

            tarifficationType = tarifficationType ?? shipping.TarifficationType;
            if (tarifficationType != null)
            {
                findParams.Add(tarifficationType.FormatEnum().Translate("ru"));
                query = query.Where(x => x.TarifficationType == tarifficationType);
            }

            if (shipping.VehicleTypeId != null)
            {
                vehicleTypesDict.TryGetValue(shipping.VehicleTypeId.Value, out VehicleType vehicleType);
                findParams.Add(vehicleType?.Name);
                query = query.Where(x => x.VehicleTypeId == null || x.VehicleTypeId == shipping.VehicleTypeId);
            }

            if (carrierId != null)
            {
                findParams.Add(carriersDict.ContainsKey(carrierId.Value) ? carriersDict[carrierId.Value].Title : string.Empty);
                query = query.Where(x => x.CarrierId == carrierId);
            }

            if (vehicleTypeIds != null && vehicleTypeIds.Any())
            {
                var vehicleTypeIdsList = vehicleTypeIds.ToList();
                findParams.AddRange(vehicleTypeIds.Select(x => vehicleTypesDict.ContainsKey(x) ? vehicleTypesDict[x].Name : string.Empty));
                query = query.Where(x => x.VehicleTypeId != null && vehicleTypeIdsList.Contains(x.VehicleTypeId.Value));
            }

            if (ignoredCarrierIds != null && ignoredCarrierIds.Any())
            {
                var ignoredCarrierIdsList = ignoredCarrierIds.ToList();
                findParams.AddRange(ignoredCarrierIds.Select(x => carriersDict.ContainsKey(x) ? $"НЕ {carriersDict[x].Title}" : string.Empty));
                query = query.Where(x => x.CarrierId != null && !ignoredCarrierIdsList.Contains(x.CarrierId.Value));
            }

            Log.Information($"Подбор тарифа. Поиск тарифа для перевозки {shipping.ShippingNumber} по параметрам: {string.Join("; ", findParams)}.");

            var allTariffs = query.ToList();

            var tariffs = allTariffs.Where(x => x.ShippingWarehouseId == firstOrder.ShippingWarehouseId
                                                && x.DeliveryWarehouseId == lastOrder.DeliveryWarehouseId)
                                    .ToList();

            string findOption = null;

            if (tariffs.Any())
            {
                findOption = "Тариф найден по складам перевозки";
            }
            else
            {
                tariffs = allTariffs.Where(x => x.ShipmentCity == firstOrder.ShippingCity 
                                                && x.DeliveryCity == lastOrder.DeliveryCity)
                                    .ToList();

                if (tariffs.Any())
                {
                    findOption = "Тариф найден по городам перевозки";
                }
                else
                {
                    tariffs = allTariffs.Where(x => x.ShipmentRegion == firstOrder.ShippingRegion
                                                    && x.DeliveryRegion == lastOrder.DeliveryRegion)
                                        .ToList();

                    if (tariffs.Any())
                    {
                        findOption = "Тариф найден по регионам перевозки";
                    }
                }
            }

            if (!tariffs.Any())
            {
                Log.Warning($"Подбор тарифа. Подходящего тарифа не найдено.");
                return null;
            }

            Tariff result;
            if (tariffs.Count == 1)
            {
                result = tariffs.First();
            }
            else
            {
                result = tariffs.OrderBy(x => GetBaseDeliveryCost(x, shipping, orders) ?? int.MaxValue).First();
            }

            var deliveryCost = GetBaseDeliveryCost(result, shipping, orders);
            Log.Information($"Подбор тарифа. {findOption}: {result.Carrier?.Title} ({deliveryCost})");

            return result;
        }

        public void UpdateDeliveryCost(Tariff tariff, Shipping shipping, IEnumerable<Order> orders = null)
        {
            if (shipping == null)
            {
                return;
            }

            if (orders == null)
            {
                orders = _commonDataService.GetDbSet<Order>()
                                           .Where(x => x.ShippingId == shipping.Id)
                                           .ToList();
            }

            if (tariff == null)
            {
                if (shipping.BasicDeliveryCostWithoutVAT != 0)
                {
                    _historyService.Save(shipping.Id, "DeliveryCost.NoTariffsWarning");
                }

                shipping.BasicDeliveryCostWithoutVAT = 0M;
                foreach (var order in orders)
                {
                    order.DeliveryCost = 0M;
                    order.ManualDeliveryCost = false;
                }

                return;
            }

            decimal? extraPointCosts;
            decimal? deliveryCost = GetDeliveryCost(tariff, shipping, orders, out extraPointCosts);

            shipping.BasicDeliveryCostWithoutVAT = deliveryCost;
            shipping.ExtraPointCostsWithoutVAT = extraPointCosts;

            var totalPallets = orders.Sum(x => x.PalletsCount ?? 0);
            foreach (var order in orders)
            {
                if (totalPallets > 0)
                {
                    order.DeliveryCost = deliveryCost * order.PalletsCount.Value / totalPallets;
                }
                else
                {
                    order.DeliveryCost = 0M;
                }
                order.ManualDeliveryCost = false;
            }
        }

        public decimal? GetDeliveryCost(Tariff tariff, Shipping shipping, IEnumerable<Order> orders, out decimal? extraPointCosts)
        {
            int extraPointsCount = orders.Select(x => x.ShippingAddress)
                                         .Where(x => !string.IsNullOrEmpty(x))
                                         .Concat(orders.Select(x => x.DeliveryAddress)
                                                       .Where(x => !string.IsNullOrEmpty(x)))
                                         .Distinct()
                                         .Count() - 2;
            extraPointCosts = extraPointsCount * tariff?.ExtraPointRate;

            return GetBaseDeliveryCost(tariff, shipping, orders);
        }

        public decimal? GetBaseDeliveryCost(Tariff tariff, Shipping shipping, IEnumerable<Order> orders)
        {
            if (tariff == null)
            {
                return null;
            }

            var totalPallets = (int)Math.Ceiling(orders.Sum(x => x.PalletsCount ?? 0));

            decimal? deliveryCost;
            if (tariff.TarifficationType == TarifficationType.Ftl
                || tariff.TarifficationType == TarifficationType.Doubledeck)
            {
                deliveryCost = tariff.FtlRate;
            }
            else if (shipping.IsPooling == true && tariff.PoolingPalletRate.HasValue)
            {
                deliveryCost = tariff.PoolingPalletRate.Value * totalPallets;
            }
            else
            {
                deliveryCost = GetLtlRate(tariff, totalPallets);
            }

            if (deliveryCost != null)
            {
                DateTime shippingDate = orders.Min(x => x.ShippingDate.Value);
                bool needWinterCoeff = tariff.StartWinterPeriod != null
                                    && tariff.EndWinterPeriod != null
                                    && shippingDate >= tariff.StartWinterPeriod
                                    && shippingDate <= tariff.EndWinterPeriod
                                    && tariff.WinterAllowance != null;
                if (needWinterCoeff)
                {
                    deliveryCost *= 1 + tariff.WinterAllowance.Value / 100;
                }
            }

            return deliveryCost;
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
    }
}
