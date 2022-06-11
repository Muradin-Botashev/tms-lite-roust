using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;

namespace Application.BusinessModels.Orders.Validation
{
    public class CarrierIdCompanyValidationRule : BaseRefCompanyValidationRule<TransportCompany>
    {
        public CarrierIdCompanyValidationRule(ICommonDataService dataService, IUserProvider userProvider) : base(dataService, userProvider)
        {
        }

        protected override string Field => nameof(OrderDto.CarrierId);

        protected override string GetMessage(string lang)
        {
            return "invalidCompanyCarrier".Translate(lang);
        }

        protected override Guid? GetRefCompanyId(TransportCompany entity)
        {
            return entity?.CompanyId;
        }

        protected override Guid? GetRefId(OrderDto dto)
        {
            return dto?.CarrierId?.Value.ToGuid();
        }
    }
}
