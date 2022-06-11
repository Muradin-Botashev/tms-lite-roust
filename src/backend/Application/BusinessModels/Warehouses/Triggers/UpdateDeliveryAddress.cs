using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Addresses;
using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;

namespace Application.BusinessModels.Warehouses.Triggers
{
    [TriggerCategory(TriggerCategory.UpdateFields)]
    public class UpdateDeliveryAddress : ITrigger<Warehouse>
    {
        private readonly ICleanAddressService _cleanAddressService;

        public UpdateDeliveryAddress(ICleanAddressService cleanAddressService)
        {
            _cleanAddressService = cleanAddressService;
        }

        public void Execute(IEnumerable<EntityChanges<Warehouse>> changes)
        {
            foreach (var change in changes)
            {
                var entity = change.Entity;
                var cleanAddress = string.IsNullOrEmpty(entity.Address) ? null : _cleanAddressService.CleanAddress(entity.Address);

                entity.ValidAddress = cleanAddress?.Address;
                entity.PostalCode = cleanAddress?.PostalCode;
                entity.Region = cleanAddress?.Region;
                entity.City = cleanAddress?.City;
                entity.Area = cleanAddress?.Area;
                entity.Street = cleanAddress?.Street;
                entity.House = cleanAddress?.House;
                entity.Latitude = cleanAddress?.Latitude;
                entity.Longitude = cleanAddress?.Longitude;
                entity.GeoQuality = cleanAddress?.GeoQuality;
                entity.UnparsedAddressParts = cleanAddress?.UnparsedAddressParts;
            }
        }

        public IEnumerable<EntityChanges<Warehouse>> FilterTriggered(IEnumerable<EntityChanges<Warehouse>> changes)
        {
            return changes.FilterChanged(x => x.Address);
        }
    }
}
