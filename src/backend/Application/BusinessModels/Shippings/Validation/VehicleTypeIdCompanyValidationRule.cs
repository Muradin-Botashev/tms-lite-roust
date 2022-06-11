using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Shippings.Validation
{
    public class VehicleTypeIdCompanyValidationRule : BaseRefCompanyValidationRule<VehicleType>
    {
        public VehicleTypeIdCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider) : base(dataService, userProvider)
        {
        }

        protected override string Field => nameof(ShippingDto.VehicleTypeId);

        protected override string GetMessage(string lang)
        {
            return "invalidCompanyVehicleType".Translate(lang);
        }

        protected override Guid? GetRefCompanyId(VehicleType entity)
        {
            return entity?.CompanyId;
        }

        protected override Guid? GetRefId(ShippingDto dto)
        {
            return dto?.VehicleTypeId?.Value.ToGuid();
        }
    }
}
