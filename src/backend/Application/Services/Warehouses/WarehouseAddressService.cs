using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.WarehouseAddress;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Services.WarehouseAddress
{
    public class WarehouseAddressService : IWarehouseAddressService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        public WarehouseAddressService(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public List<LookUpDto> ForSelect(WarehouseAddressFilter request)
        {
            var companyId = request?.CompanyId?.ToGuid() ?? _userProvider.GetCurrentUser()?.CompanyId;
            Expression<Func<Warehouse, bool>> predicate;

            if (string.IsNullOrEmpty(request.ClientName))
            {
                predicate = x => x.CompanyId == null || companyId == null || x.CompanyId == companyId;
            }
            else
            {
                predicate = x => (x.CompanyId == null || companyId == null || x.CompanyId == companyId) && x.Client == request.ClientName;
            }

            List<LookUpDto> result = _dataService.GetDbSet<Warehouse>()
                .Where(predicate)
                .Select(i => i.Address)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Distinct()
                .Select(i => new LookUpDto(i))
                .OrderBy(i => i.Name)
                .ToList();

            return result;
        }
    }
}