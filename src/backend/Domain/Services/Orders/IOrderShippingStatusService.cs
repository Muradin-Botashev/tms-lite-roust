using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Orders
{
    public interface IOrderShippingStatusService
    {
        IEnumerable<StateDto> GetAll();
        IEnumerable<LookUpDto> ForSelect();
    }
}
