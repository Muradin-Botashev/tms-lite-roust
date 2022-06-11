using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.ShippingWarehouses;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/shippingWarehouses")]
    public class ShippingWarehousesController : DictionaryController<IShippingWarehousesService, ShippingWarehouse, ShippingWarehouseDto, ShippingWarehouseFilterDto> 
    {
        public ShippingWarehousesController(IShippingWarehousesService warehousesService, IAppConfigurationService appConfigurationService) : base(warehousesService, appConfigurationService)
        {
        }
    }
}