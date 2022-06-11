using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Orders.Validation
{
    public class VehicleTypeIdCompanyValidationRule : BaseRefCompanyValidationRule<VehicleType>
    {
        public VehicleTypeIdCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider) : base(dataService, userProvider)
        {
        }

        protected override string Field => nameof(OrderDto.VehicleTypeId);

        protected override string GetMessage(string lang)
        {
            return "invalidCompanyVehicleType".Translate(lang);
        }

        protected override Guid? GetRefCompanyId(VehicleType entity)
        {
            return entity?.CompanyId;
        }

        protected override Guid? GetRefId(OrderDto dto)
        {
            return dto?.VehicleTypeId?.Value.ToGuid();
        }
    }
}
