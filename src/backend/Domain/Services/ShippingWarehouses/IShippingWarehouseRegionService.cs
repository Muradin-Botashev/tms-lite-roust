using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.ShippingWarehouseRegion
{
    public interface IShippingWarehouseRegionService
    {
        IEnumerable<LookUpDto> ForSelect(Guid? companyId = null);
    }
}
