using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Enum
{
    [Route("api/orderType")]
    public class OrderTypeController : EnumController<OrderType>
    {
        
    }
}