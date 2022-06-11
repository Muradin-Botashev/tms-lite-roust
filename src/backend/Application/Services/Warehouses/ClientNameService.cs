using DAL.Services;
using Domain.Persistables;
using Domain.Services.Warehouses;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Warehouses
{
    public class ClientNameService : IClientNameService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        public ClientNameService(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public IEnumerable<LookUpDto> ForSelect(Guid? companyId = null)
        {
            companyId = companyId ?? _userProvider.GetCurrentUser()?.CompanyId;
            var result = _dataService.GetDbSet<Warehouse>()
                                          .Where(x => x.CompanyId == null || companyId == null || x.CompanyId == companyId)
                                          .Select(w => w.Client)
                                          .Where(i => !string.IsNullOrWhiteSpace(i))
                                          .Distinct()
                                          .Select(i => new LookUpDto(i))
                                          .OrderBy(i => i.Name)
                                          .ToList();
            return result;
        }
    }
}