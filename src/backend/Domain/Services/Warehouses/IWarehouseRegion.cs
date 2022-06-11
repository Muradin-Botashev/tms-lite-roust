using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.WarehouseRegion
{
    public interface IWarehouseRegionService
    {
        IEnumerable<LookUpDto> ForSelect(Guid? companyId = null);
    }
}
