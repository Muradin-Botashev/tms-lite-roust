using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Warehouses;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/warehouses")]
    public class WarehousesController : DictionaryController<IWarehousesService, Warehouse, WarehouseDto, WarehouseFilterDto> 
    {
        public WarehousesController(IWarehousesService warehousesService, IAppConfigurationService appConfigurationService) : base(warehousesService, appConfigurationService)
        {
        }
    }
}