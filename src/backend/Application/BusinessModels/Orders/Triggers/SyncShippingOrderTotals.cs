using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncShippingOrderTotals : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;

        public SyncShippingOrderTotals(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var orderIds = changes.Select(x => x.Entity.Id).ToList();
            var shippingIds = changes.Select(x => x.Entity.ShippingId).Where(x => x != null).ToList();

            var shippings = _dataService.GetDbSet<Shipping>()
                                        .Where(x => shippingIds.Contains(x.Id))
                                        .ToList();

            var ordersDict = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId != null
                                                && shippingIds.Contains(x.ShippingId.Value)
                                                && !orderIds.Contains(x.Id))
                                         .GroupBy(x => x.ShippingId)
                                         .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var entity in changes.Select(x => x.Entity).Where(x => x.ShippingId != null))
            {
                List<Order> shippingOrders;
                if (!ordersDict.TryGetValue(entity.ShippingId, out shippingOrders))
                {
                    shippingOrders = new List<Order>();
                    ordersDict[entity.ShippingId] = shippingOrders;
                }
                shippingOrders.Add(entity);
            }

            foreach (var shipping in shippings)
            {
                List<Order> orders = null;
                ordersDict.TryGetValue(shipping.Id, out orders);

                shipping.ShippingDate = orders.Select(x => x.ShippingDate).Min();
                shipping.DeliveryDate = orders.Select(x => x.DeliveryDate).Max();

                var loadingArrivalTimes = orders
                    .Where(i => i.LoadingArrivalTime != null)
                    .Select(i => i.LoadingArrivalTime);
                shipping.LoadingArrivalTime = loadingArrivalTimes.Any() ? loadingArrivalTimes.Min() : null;

                var loadingDepartureTimes = orders
                    .Where(i => i.LoadingDepartureTime != null)
                    .Select(i => i.LoadingDepartureTime);
                shipping.LoadingDepartureTime = loadingDepartureTimes.Any() ? loadingDepartureTimes.Min() : null;

                if (!shipping.ManualActualPalletsCount)
                {
                    var counts = orders.Select(o => o.ActualPalletsCount).ToList();
                    shipping.ActualPalletsCount = counts.Any(x => x.HasValue) ? (int)Math.Ceiling(counts.Sum(x => x ?? 0M)) : (int?)null;
                }

                if (!shipping.ManualActualWeightKg)
                {
                    var weights = orders.Select(o => o.ActualWeightKg).ToList();
                    shipping.ActualWeightKg = weights.Any(x => x.HasValue) ? weights.Sum(x => x ?? 0) : (decimal?)null;
                }

                if (!shipping.ManualConfirmedPalletsCount)
                {
                    var counts = orders.Select(o => o.ConfirmedPalletsCount).ToList();
                    shipping.ConfirmedPalletsCount = counts.Any(x => x.HasValue) ? (int)Math.Ceiling(counts.Sum(x => x ?? 0M)) : (int?)null;
                }

                if (!shipping.ManualPalletsCount)
                {
                    var counts = orders.Select(o => o.PalletsCount).ToList();
                    shipping.PalletsCount = counts.Any(x => x.HasValue) ? (int)Math.Ceiling(counts.Sum(x => x ?? 0M)) : (int?)null;
                }

                if (!shipping.ManualTrucksDowntime)
                {
                    var downtimes = orders.Select(o => o.TrucksDowntime).ToList();
                    shipping.TrucksDowntime = downtimes.Any(x => x.HasValue) ? downtimes.Sum(x => x ?? 0) : (decimal?)null;
                }

                if (!shipping.ManualWeightKg)
                {
                    var weights = orders.Select(o => o.WeightKg).ToList();
                    shipping.WeightKg = weights.Any(x => x.HasValue) ? weights.Sum(x => x ?? 0) : (decimal?)null;
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(
                x => x.ActualPalletsCount,
                x => x.ActualWeightKg,
                x => x.ConfirmedPalletsCount,
                x => x.DeliveryDate,
                x => x.LoadingArrivalTime,
                x => x.LoadingDepartureTime,
                x => x.PalletsCount,
                x => x.ShippingDate,
                x => x.TrucksDowntime,
                x => x.WeightKg);
        }
    }
}
