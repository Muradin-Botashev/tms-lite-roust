using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.Shippings
{
    public class ShippingChangesService : IShippingChangesService
    {
        private readonly ICommonDataService _dataService;

        public ShippingChangesService(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public bool ClearBacklightFlags(IEnumerable<Shipping> entities, Role role)
        {
            bool result = false;

            if (role?.Backlights != null
                && role.Backlights.Contains((int)BacklightType.CarrierRequestSentBacklight))
            {
                var entitiesToUpdate = entities.Where(x => x.IsNewCarrierRequest);
                if (entitiesToUpdate.Any())
                {
                    foreach (var entity in entitiesToUpdate)
                    {
                        entity.IsNewCarrierRequest = false;
                    }

                    var entitieIds = entitiesToUpdate.Select(x => x.Id).ToList();
                    var orders = _dataService.GetDbSet<Order>()
                                             .Where(x => x.ShippingId != null
                                                        && entitieIds.Contains(x.ShippingId.Value)
                                                        && x.IsNewCarrierRequest)
                                             .ToList();

                    foreach (var order in orders)
                    {
                        order.IsNewCarrierRequest = false;
                    }

                    result = true;
                }
            }

            return result;
        }
    }
}
