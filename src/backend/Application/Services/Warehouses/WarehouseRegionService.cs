using DAL.Services;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using Domain.Services.WarehouseRegion;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.WarehouseCity
{
    public class WarehouseRegionService : IWarehouseRegionService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        public WarehouseRegionService(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public IEnumerable<LookUpDto> ForSelect(Guid? companyId = null)
        {
            companyId = companyId ?? _userProvider.GetCurrentUser()?.CompanyId;
            return _dataService.GetDbSet<Warehouse>()
                .Where(x => x.CompanyId == null || companyId == null || x.CompanyId == companyId)
                .Select(i => i.Region)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Distinct()
                .OrderBy(i => i)
                .Select(i => new LookUpDto
                { 
                    Value = i,
                    Name = i
                })
                .ToList();
        }
    }
}
