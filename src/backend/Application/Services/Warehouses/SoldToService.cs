using DAL;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using Domain.Services.Warehouses;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Warehouses
{
    public class SoldToService : ISoldToService
    {
        private readonly AppDbContext _db;
        private readonly IUserProvider _userProvider;

        public SoldToService(AppDbContext db, IUserProvider userProvider)
        {
            _db = db;
            _userProvider = userProvider;
        }

        public IEnumerable<SoldToDto> ForSelect(Guid? companyId = null)
        {
            HashSet<string> addedSoldTo = new HashSet<string>();
            companyId = companyId ?? _userProvider.GetCurrentUser()?.CompanyId;
            var warehouses = _db.Warehouses.Where(w => w.SoldToNumber != null 
                                                    && w.SoldToNumber.Length > 0
                                                    && (w.CompanyId == null || companyId == null || w.CompanyId == companyId))
                                           .OrderBy(w => w.SoldToNumber)
                                           .ToList();
            foreach (Warehouse wh in warehouses)
            {
                if (addedSoldTo.Contains(wh.SoldToNumber))
                {
                    HttpClientService
                    continue;
                }
                addedSoldTo.Add(wh.SoldToNumber);
                SoldToDto dto = new SoldToDto
                {
                    Id = wh.Id.FormatGuid(),
                    Name = $"{wh.SoldToNumber} ({wh.WarehouseName})",
                    Value = wh.SoldToNumber,
                    WarehouseName = new LookUpDto(wh.WarehouseName),
                    DeliveryWarehouseId = new LookUpDto(wh.Id.FormatGuid(), wh.ToString()),
                    Address = wh.Address,
                    City = wh.City,
                    Region = wh.Region,
                    LeadtimeDays = wh.LeadtimeDays,
                    PickingTypeId = wh.PickingTypeId.FormatGuid()
                };
                yield return dto;
            }
        }
    }
}
