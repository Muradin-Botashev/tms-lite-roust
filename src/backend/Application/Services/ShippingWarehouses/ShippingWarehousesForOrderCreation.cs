using DAL;
using Domain.Extensions;
using Domain.Services.ShippingWarehouses;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ShippingWarehouses
{
    public class ShippingWarehousesForOrderCreation : IShippingWarehousesForOrderCreation
    {
        private readonly AppDbContext _db;
        private readonly IUserProvider _userProvider;

        public ShippingWarehousesForOrderCreation(AppDbContext db, IUserProvider userProvider)
        {
            _db = db;
            _userProvider = userProvider;
        }

        public IEnumerable<ShippingWarehouseDtoForSelect> ForSelect(Guid? companyId = null)
        {
            companyId = companyId ?? _userProvider.GetCurrentUser()?.CompanyId;
            var warehouses = _db.ShippingWarehouses.Where(x => x.IsActive && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                                   .OrderBy(w => w.WarehouseName)
                                                   .ToList();
            foreach (var wh in warehouses)
            {
                var dto = new ShippingWarehouseDtoForSelect
                {
                    Name = wh.WarehouseName,
                    Address = wh.Address,
                    Value = wh.Id.FormatGuid(),
                };
                yield return dto;
            }
        }
    }
}
