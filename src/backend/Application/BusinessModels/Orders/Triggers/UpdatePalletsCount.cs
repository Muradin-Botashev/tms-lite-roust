using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdatePalletsCount : ITrigger<Order>
    {
        private readonly ICommonDataService _dataService;
        private readonly IShippingTarifficationTypeDeterminer _shippingTarifficationTypeDeterminer;
        private readonly IHistoryService _historyService; 

        public UpdatePalletsCount(
            ICommonDataService dataService, 
            IShippingTarifficationTypeDeterminer shippingTarifficationTypeDeterminer, 
            IHistoryService historyService)
        {
            _dataService = dataService;
            _shippingTarifficationTypeDeterminer = shippingTarifficationTypeDeterminer;
            _historyService = historyService;
        }

        public void Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            changes = changes.Where(entity => entity.Entity.Status == OrderState.Draft 
                                            || entity.Entity.Status == OrderState.Created 
                                            || entity.Entity.Status == OrderState.Confirmed 
                                            || entity.Entity.Status == OrderState.InShipping);
            
            var orderIds = changes.Select(x => x.Entity.Id).ToList();
            var shippingIds = changes.Select(x => x.Entity.ShippingId).Where(x => x != null).ToList();

            var shippingsDict = _dataService.GetDbSet<Shipping>()
                                            .Where(x => shippingIds.Contains(x.Id))
                                            .ToDictionary(x => x.Id);

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

            foreach (var entity in changes.Select(x => x.Entity))
            {
                if (entity.ShippingId.HasValue)
                {
                    Shipping shipping = null;
                    shippingsDict.TryGetValue(entity.ShippingId.Value, out shipping);

                    List<Order> orders = null;
                    ordersDict.TryGetValue(shipping.Id, out orders);

                    if (shipping.Status == ShippingState.ShippingCreated && !shipping.ManualTarifficationType)
                    {
                        var tarifficationType = _shippingTarifficationTypeDeterminer.GetTarifficationTypeForOrders(shipping, orders);

                        foreach (var orderInShipping in orders)
                        {
                            if (orderInShipping.TarifficationType != tarifficationType)
                            {
                                _historyService.Save(orderInShipping.Id, "fieldChangedBy",
                                    nameof(orderInShipping.TarifficationType).ToLowerFirstLetter(),
                                    orderInShipping.TarifficationType, tarifficationType, "onChangePalletsCountOrDeliveryRegionInOtherOrderInShipping");

                                orderInShipping.TarifficationType = tarifficationType;
                            }
                        }

                        if (shipping.TarifficationType != tarifficationType)
                        {
                            _historyService.Save(shipping.Id, "fieldChangedBy",
                                nameof(shipping.TarifficationType).ToLowerFirstLetter(),
                                shipping.TarifficationType, tarifficationType, "onChangePalletsCountOrDeliveryRegionInIncludedOrder");

                            shipping.TarifficationType = tarifficationType;
                        }
                    }
                }
                else
                {
                    var tarifficationType = _shippingTarifficationTypeDeterminer.GetTarifficationTypeForOrders(null, new[] { entity });
                    if (entity.TarifficationType != tarifficationType)
                    {
                        _historyService.Save(entity.Id, "fieldChangedBy",
                            nameof(entity.TarifficationType).ToLowerFirstLetter(),
                            entity.TarifficationType, tarifficationType, "onChangePalletsCountOrDeliveryRegion");

                        entity.TarifficationType = tarifficationType;
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes.FilterChanged(x => x.PalletsCount);
        }
    }
}
