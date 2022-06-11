using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using System;
using System.Collections.Generic;

namespace Application.BusinessModels.Orders.Actions
{
    public abstract class BaseOrderAction
    {
        protected readonly ICommonDataService _dataService;
        protected readonly Dictionary<Guid, Company> _companiesCache;

        public BaseOrderAction(ICommonDataService dataService)
        {
            _dataService = dataService;
            _companiesCache = new Dictionary<Guid, Company>();
        }

        protected Company GetCompany(Order order)
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

        protected bool IsConfirmedOrder(Order order)
        {
            var company = GetCompany(order);
            if (company?.OrderRequiresConfirmation == true)
                return order.Status == OrderState.Confirmed;
            else
                return order.Status == OrderState.Created;
        }
    }
}
