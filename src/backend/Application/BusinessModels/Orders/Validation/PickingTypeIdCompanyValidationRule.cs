using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Orders.Validation
{
    public class PickingTypeIdCompanyValidationRule : BaseRefCompanyValidationRule<PickingType>
    {
        public PickingTypeIdCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider) : base(dataService, userProvider)
        {
        }

        protected override string Field => nameof(OrderDto.PickingTypeId);

        protected override string GetMessage(string lang)
        {
            return "invalidCompanyPickingType".Translate(lang);
        }

        protected override Guid? GetRefCompanyId(PickingType entity)
        {
            return entity?.CompanyId;
        }

        protected override Guid? GetRefId(OrderDto dto)
        {
            return dto?.PickingTypeId?.Value.ToGuid();
        }
    }
}
