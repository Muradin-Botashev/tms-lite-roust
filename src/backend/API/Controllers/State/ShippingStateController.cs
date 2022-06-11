using Domain.Enums;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.State
{
    [Route("api/shippingState")]
    public class ShippingStateController : StateController<ShippingState>
    {
        public ShippingStateController(IStateService stateService) : base(stateService)
        {
        }
    }
}