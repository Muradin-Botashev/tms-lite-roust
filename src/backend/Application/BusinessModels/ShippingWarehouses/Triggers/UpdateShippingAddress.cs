using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Addresses;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.ShippingWarehouses.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateShippingAddress : ITrigger<ShippingWarehouse>
    {
        private readonly ICleanAddressService _cleanAddressService;

        public UpdateShippingAddress(ICleanAddressService cleanAddressService)
        {
            _cleanAddressService = cleanAddressService;
        }

        public void Execute(IEnumerable<EntityChanges<ShippingWarehouse>> changes)
        {
            foreach (var entity in changes.Select(x => x.Entity))
            {
                string rawAddress = $"{entity.City} {entity.Address}";
                var cleanAddress = string.IsNullOrEmpty(rawAddress) ? null : _cleanAddressService.CleanAddress(rawAddress);

                entity.ValidAddress = cleanAddress?.Address;
                entity.PostalCode = cleanAddress?.PostalCode;
                entity.Region = cleanAddress?.Region;
                entity.Area = cleanAddress?.Area;
                entity.Street = cleanAddress?.Street;
                entity.House = cleanAddress?.House;
                entity.Latitude = cleanAddress?.Latitude;
                entity.Longitude = cleanAddress?.Longitude;
                entity.GeoQuality = cleanAddress?.GeoQuality;
            }
        }

        public IEnumerable<EntityChanges<ShippingWarehouse>> FilterTriggered(IEnumerable<EntityChanges<ShippingWarehouse>> changes)
        {
            return changes.FilterChanged(
                x => x.City,
                x => x.Address);
        }
    }
}
