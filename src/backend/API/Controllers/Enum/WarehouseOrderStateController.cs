using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Enum
{
    [Route("api/warehouseOrderState")]
    public class WarehouseOrderStateController : EnumController<WarehouseOrderState>
    {
        
    }
}