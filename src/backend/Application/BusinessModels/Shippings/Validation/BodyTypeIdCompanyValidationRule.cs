using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Shippings.Validation
{
    public class BodyTypeIdCompanyValidationRule : BaseRefCompanyValidationRule<BodyType>
    {
        public BodyTypeIdCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider) : base(dataService, userProvider)
        {
        }

        protected override string Field => nameof(ShippingDto.BodyTypeId);

        protected override string GetMessage(string lang)
        {
            return "invalidCompanyBodyType".Translate(lang);
        }

        protected override Guid? GetRefCompanyId(BodyType entity)
        {
            return entity?.CompanyId;
        }

        protected override Guid? GetRefId(ShippingDto dto)
        {
            return dto?.BodyTypeId?.Value.ToGuid();
        }
    }
}
