using DAL.Services;
using Domain.Persistables;
using System;
using System.Collections.Generic;

namespace Application.BusinessModels.Shippings.Actions
{
    public class BaseShippingAction
    {
        protected readonly ICommonDataService _dataService;
        protected readonly Dictionary<Guid, Company> _companiesCache;

        public BaseShippingAction(ICommonDataService dataService)
        {
            _dataService = dataService;
            _companiesCache = new Dictionary<Guid, Company>();
        }

        protected Company GetOrderCompany(Order order)
        {
            if (order.Company != null)
                return order.Company;

            if (order.CompanyId == null)
                return null;

            Company result;
            if (!_companiesCache.TryGetValue(order.CompanyId.Value, out result))
            {
                result = _dataService.GetById<Company>(order.CompanyId.Value);
                _companiesCache[order.CompanyId.Value] = result;
            }

            return result;
        }
    }
}
