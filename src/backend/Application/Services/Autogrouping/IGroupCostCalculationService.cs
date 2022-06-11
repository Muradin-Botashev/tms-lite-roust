using Domain.Enums;
using System.Collections.Generic;

namespace Application.Services.Autogrouping
{
    public interface IGroupCostCalculationService
    {
        void FillCosts(IEnumerable<ShippingRoute> routes, List<AutogroupingType> types);
    }
}