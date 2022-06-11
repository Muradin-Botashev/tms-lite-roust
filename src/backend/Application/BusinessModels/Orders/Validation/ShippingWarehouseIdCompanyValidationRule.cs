using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Orders.Validation
{
    public class ShippingWarehouseIdCompanyValidationRule : BaseRefCompanyValidationRule<ShippingWarehouse>
    {
        public ShippingWarehouseIdCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider) : base(dataService, userProvider)
        {
        }

        protected override string Field => nameof(OrderDto.ShippingWarehouseId);

        protected override string GetMessage(string lang)
        {
            return "invalidCompanyShippingWarehouse".Translate(lang);
        }

        protected override Guid? GetRefCompanyId(ShippingWarehouse entity)
        {
            return entity?.CompanyId;
        }

        protected override Guid? GetRefId(OrderDto dto)
        {
            return dto?.ShippingWarehouseId?.Value.ToGuid();
        }
    }
}
