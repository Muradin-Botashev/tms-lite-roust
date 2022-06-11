using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.WarehouseCity
{
    public interface IWarehouseCityService
    {
        IEnumerable<LookUpDto> ForSelect(Guid? companyId = null);
    }
}
