using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Orders;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Orders.Triggers
{
    public class ValidateClearBacklightFlags : IValidationTrigger<Order>
    {
        private readonly IUserProvider _userProvider;
        private readonly ICommonDataService _dataService;
        private readonly IOrderChangesService _orderChangesService;

        public ValidateClearBacklightFlags(
            IUserProvider userProvider, 
            ICommonDataService dataService, 
            IOrderChangesService orderChangesService)
        {
            _userProvider = userProvider;
            _dataService = dataService;
            _orderChangesService = orderChangesService;
        }

        public ValidateResult Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var user = _userProvider.GetCurrentUser();
            var role = user?.RoleId == null ? null : _dataService.GetById<Role>(user.RoleId.Value);
            var entities = changes.Select(x => x.Entity).ToList();
            _orderChangesService.ClearBacklightFlags(entities, role);
            return new ValidateResult();
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes;
        }
    }
}
