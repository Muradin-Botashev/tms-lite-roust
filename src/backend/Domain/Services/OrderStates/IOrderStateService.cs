using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.OrderStates
{
    public interface IOrderStateService
    {
        IEnumerable<StateDto> GetAll();
        IEnumerable<LookUpDto> ForSelect();
    }
}
