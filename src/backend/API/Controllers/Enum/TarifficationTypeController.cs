using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Enum
{
    [Route("api/tarifficationType")]
    public class TarifficationTypeController : EnumController<TarifficationType>
    {
        
    }
}