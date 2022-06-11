using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.WarehouseAddress
{
    public interface IWarehouseAddressService
    {
        List<LookUpDto> ForSelect(WarehouseAddressFilter request);
    }
}