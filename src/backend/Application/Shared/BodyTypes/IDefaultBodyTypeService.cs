using Domain.Persistables;
using System;

namespace Application.Shared.BodyTypes
{
    public interface IDefaultBodyTypeService
    {
        BodyType GetDefaultBodyType(Order order);
        BodyType GetDefaultBodyType(
            DateTime? shippingDate,
            Guid? shippingWarehouseId,
            Guid? deliveryWarehouseId,
            string shippingCity,
            string deliveryCity,
            string shippingRegion,
            string deliveryRegion,
            Guid? companyId);
    }
}