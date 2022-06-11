using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Enum
{
    [Route("api/weekDay")]
    public class WeekDayController : EnumController<WeekDay>
    {
    }
}