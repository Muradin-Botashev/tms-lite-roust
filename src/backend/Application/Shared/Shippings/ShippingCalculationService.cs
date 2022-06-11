using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.Shippings
{
    public class ShippingCalculationService : IShippingCalculationService
    {
        private readonly ICommonDataService _dataService;
        private readonly IDeliveryCostCalcService _costCalcService;

        public ShippingCalculationService(ICommonDataService dataService, IDeliveryCostCalcService costCalcService)
        {
            _dataService = dataService;
            _costCalcService = costCalcService;
        }

        public void RecalculateDeliveryCosts(Shipping shipping, IEnumerable<Order> orders)
        {
            orders = EnsureShippingOrders(shipping, orders);
            shipping.ReturnCostWithoutVAT = orders.Sum(i => i.ReturnShippingCost ?? 0M);
            RecalculateTotalCosts(shipping, orders);
        }

        public void RecalculateTotalCosts(Shipping shipping, IEnumerable<Order> orders)
        {
            decimal newTotalValue = (shipping.BasicDeliveryCostWithoutVAT ?? 0M)
                                    + (shipping.DowntimeRate ?? 0M)
                                    + (shipping.ReturnCostWithoutVAT ?? 0M)
                                    + (shipping.ExtraPointCostsWithoutVAT ?? 0M)
                                    + (shipping.OtherCosts ?? 0M);

            shipping.TotalDeliveryCostWithoutVAT = newTotalValue;
            shipping.TotalDeliveryCost = shipping.TotalDeliveryCostWithoutVAT * 1.2M;

            orders = EnsureShippingOrders(shipping, orders);

            var totalPallets = orders.Sum(x => x.PalletsCount ?? 0);

            if (totalPallets > 0)
            {
                orders.ToList().ForEach(i => i.TotalAmount = shipping.TotalDeliveryCostWithoutVAT * i.PalletsCount / totalPallets);
                orders.ToList().ForEach(i => i.TotalAmountNds = shipping.TotalDeliveryCost * i.PalletsCount / totalPallets);
            }
        }

        public void RecalculateShippingOrdersCosts(Shipping shipping, IEnumerable<Order> orders)
        {
            orders = EnsureShippingOrders(shipping, orders);

            var totalPallets = orders.Sum(x => x.PalletsCount ?? 0);

            if (totalPallets > 0)
            {
                orders.ToList().ForEach(i => i.OtherExpenses = shipping.OtherCosts * i.PalletsCount / totalPallets);
                orders.ToList().ForEach(i => i.DowntimeAmount = shipping.DowntimeRate * i.PalletsCount / totalPallets);
            }
        }

        public void ClearShippingOrdersCosts(IEnumerable<Order> orders)
        {
            if (orders != null)
            {
                foreach (var order in orders)
                {
                    order.DowntimeAmount = null;
                    order.OtherExpenses = null;
                    order.ReturnShippingCost = null;
                    order.TotalAmount = null;
                    order.TotalAmountNds = null;
                }
            }
        }

        public void RecalculateTemperature(Shipping shipping, IEnumerable<Order> orders)
        {
            orders = EnsureShippingOrders(shipping, orders);

            var tempRange = FindCommonTempRange(orders);
            shipping.TemperatureMin = tempRange?.Key;
            shipping.TemperatureMax = tempRange?.Value;
        }

        public void RecalculateShipping(Shipping shipping, IEnumerable<Order> orders)
        {
            orders = EnsureShippingOrders(shipping, orders);

            RecalculateTemperature(shipping, orders);

            decimal? downtime = orders.Any(o => o.TrucksDowntime.HasValue)
                ? orders.Sum(o => o.TrucksDowntime ?? 0)
                : (decimal?)null;
            int? palletsCount = orders.Any(o => o.PalletsCount.HasValue) ? (int)Math.Ceiling(orders.Sum(o => o.PalletsCount ?? 0)) : (int?)null;
            int? actualPalletsCount = orders.Any(o => o.ActualPalletsCount.HasValue)
                ? (int)Math.Ceiling(orders.Sum(o => o.ActualPalletsCount ?? 0))
                : (int?)null;
            int? confirmedPalletsCount = orders.Any(o => o.ConfirmedPalletsCount.HasValue)
                ? (int)Math.Ceiling(orders.Sum(o => o.ConfirmedPalletsCount ?? 0))
                : (int?)null;
            decimal? weight = orders.Any(o => o.WeightKg.HasValue) ? orders.Sum(o => o.WeightKg ?? 0) : (decimal?)null;
            decimal? actualWeight = orders.Any(o => o.ActualWeightKg.HasValue)
                ? orders.Sum(o => o.ActualWeightKg ?? 0)
                : (decimal?)null;

            shipping.PalletsCount = palletsCount;
            shipping.ActualPalletsCount = actualPalletsCount;
            shipping.ConfirmedPalletsCount = confirmedPalletsCount;
            shipping.WeightKg = weight;
            shipping.ActualWeightKg = actualWeight;
            shipping.TrucksDowntime = downtime;

            SyncVehicleType(shipping, orders);

            var firstOrder = orders.OrderBy(x => x.ShippingDate).FirstOrDefault();
            shipping.ShippingAddress = firstOrder?.ShippingAddress;
            shipping.ShippingWarehouseId = firstOrder?.ShippingWarehouseId;

            var lastOrder = orders.OrderByDescending(x => x.DeliveryDate).FirstOrDefault();
            shipping.DeliveryAddress = lastOrder?.DeliveryAddress;
            shipping.DeliveryWarehouseId = lastOrder?.DeliveryWarehouseId;

            var loadingArrivalTimes = orders
                .Where(i => i.LoadingArrivalTime != null)
                .Select(i => i.LoadingArrivalTime);
            var loadingArrivalTime = loadingArrivalTimes.Any() ? loadingArrivalTimes.Min() : null;

            shipping.LoadingArrivalTime = loadingArrivalTime;

            var loadingDepartureTimes = orders
                .Where(i => i.LoadingDepartureTime != null)
                .Select(i => i.LoadingDepartureTime);
            var loadingDepartureTime = loadingDepartureTimes.Any() ? loadingDepartureTimes.Min() : null;

            shipping.LoadingDepartureTime = loadingDepartureTime;

            var shippingDates = orders
                .Where(i => i.ShippingDate != null)
                .Select(i => i.ShippingDate);
            var shippingDate = shippingDates.Any() ? shippingDates.Min() : null;

            shipping.ShippingDate = shippingDate;

            var deliveryDates = orders
                .Where(i => i.DeliveryDate != null)
                .Select(i => i.DeliveryDate);
            var deliveryDate = deliveryDates.Any() ? deliveryDates.Max() : null;

            shipping.DeliveryDate = deliveryDate;
        }

        public void SyncVehicleType(Shipping shipping, IEnumerable<Order> orders)
        {
            if (shipping.Status == ShippingState.ShippingCreated || shipping.Status == ShippingState.ShippingRequestSent)
            {
                orders = EnsureShippingOrders(shipping, orders);

                var isInterregion = orders.SelectMany(x => new[] { x.ShippingRegion, x.DeliveryRegion }).Distinct().Count() > 1;
                var bestVehicleType = _dataService.GetDbSet<VehicleType>()
                                                  .Include(x => x.Tonnage)
                                                  .Where(x => x.BodyTypeId == shipping.BodyTypeId 
                                                            && (x.CompanyId == null || x.CompanyId == shipping.CompanyId)
                                                            && (x.IsInterregion == true || !isInterregion)
                                                            && x.PalletsCount >= shipping.PalletsCount
                                                            && x.Tonnage.WeightKg >= shipping.WeightKg)
                                                  .OrderBy(x => x.PalletsCount)
                                                  .FirstOrDefault();
                if (bestVehicleType != null && shipping.VehicleTypeId != bestVehicleType.Id)
                {
                    shipping.VehicleTypeId = bestVehicleType.Id;

                    foreach (var order in orders)
                    {
                        order.VehicleTypeId = bestVehicleType.Id;
                    }

                    _costCalcService.UpdateDeliveryCost(shipping, orders);
                    RecalculateDeliveryCosts(shipping, orders);
                }
            }
        }

        private IEnumerable<Order> EnsureShippingOrders(Shipping shipping, IEnumerable<Order> orders)
        {
            if (orders == null)
            {
                orders = _dataService.GetDbSet<Order>()
                                     .Where(x => x.ShippingId == shipping.Id)
                                     .ToList();
            }
            return orders;
        }

        private KeyValuePair<int, int>? FindCommonTempRange(IEnumerable<Order> orders)
        {
            if (orders == null || !orders.Any() || orders.Any(o => o.TemperatureMin == null || o.TemperatureMax == null))
            {
                return null;
            }

            Order firstOrder = orders.First();
            KeyValuePair<int, int> result = new KeyValuePair<int, int>(firstOrder.TemperatureMin.Value, firstOrder.TemperatureMax.Value);

            foreach (Order order in orders.Skip(1))
            {
                if (order.TemperatureMin > result.Value || order.TemperatureMax < result.Key)
                {
                    return null;
                }
                result = new KeyValuePair<int, int>(Math.Max(result.Key, order.TemperatureMin.Value), Math.Min(result.Value, order.TemperatureMax.Value));
            }

            return result;
        }
    }
}

