using Domain.Persistables;
using System.Collections.Generic;

namespace Application.Shared.Shippings
{
    public interface IShippingChangesService
    {
        bool ClearBacklightFlags(IEnumerable<Shipping> entities, Role role);
    }
}