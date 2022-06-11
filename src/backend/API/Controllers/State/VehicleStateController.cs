using Domain.Enums;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.State
{
    [Route("api/vehicleState")]
    public class VehicleStateController : StateController<VehicleState>
    {
        public VehicleStateController(IStateService stateService) : base(stateService)
        {
        }
    }
}