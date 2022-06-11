using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Orders;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    [TriggerCategory(TriggerCategory.PostUpdates)]
    public class SendChangesToPooling : ITrigger<Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly IOrderPoolingService _poolingService;

        public SendChangesToPooling(ICommonDataService dataService, IOrderPoolingService poolingService)
        {
            _dataService = dataService;
            _poolingService = poolingService;
        }

        public void Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            var shippingIds = changes.Select(x => x.Entity.Id).ToList();
            var ordersDict = _dataService.GetDbSet<Order>()
                                         .Where(x => x.ShippingId != null && shippingIds.Contains(x.ShippingId.Value))
                                         .GroupBy(x => x.ShippingId)
                                         .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var change in changes)
            {
                var shipping = change.Entity;
                if (shipping.Status == ShippingState.ShippingSlotBooked)
                {
                    List<Order> orders = null;
                    ordersDict.TryGetValue(shipping.Id, out orders);

                    var result = _poolingService.UpdateSlot(shipping, orders);
                    if (result.IsError)
                    {
                        Log.Error($"Ошибка обновления брони пулинга перевозки {shipping.ShippingNumber} по полям: { result.Error }");
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes.FilterChanged(
                x => x.TemperatureMax,
                x => x.TemperatureMax,
                x => x.VehicleTypeId);
        }
    }
}
