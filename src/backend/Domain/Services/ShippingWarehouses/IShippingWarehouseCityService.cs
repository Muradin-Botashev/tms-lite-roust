using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.ShippingWarehouseCity
{
    public interface IShippingWarehouseCityService
    {
        IEnumerable<LookUpDto> ForSelect(Guid? companyId = null);
    }
}
