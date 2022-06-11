using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Enum
{
    [Route("api/deliveryType")]
    public class DeliveryTypeController : EnumController<DeliveryType>
    {
        
    }
}