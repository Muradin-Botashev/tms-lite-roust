using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Shippings.Validation
{
    public class CarrierIdCompanyValidationRule : BaseRefCompanyValidationRule<TransportCompany>
    {
        public CarrierIdCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider) : base(dataService, userProvider)
        {
        }

        protected override string Field => nameof(ShippingDto.CarrierId);

        protected override string GetMessage(string lang)
        {
            return "invalidCompanyCarrier".Translate(lang);
        }

        protected override Guid? GetRefCompanyId(TransportCompany entity)
        {
            return entity?.CompanyId;
        }

        protected override Guid? GetRefId(ShippingDto dto)
        {
            return dto?.CarrierId?.Value.ToGuid();
        }
    }
}
