using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Persistables;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Shippings.Triggers
{
    public class ValidateClearBacklightFlags : IValidationTrigger<Shipping>
    {
        private readonly IUserProvider _userProvider;
        private readonly ICommonDataService _dataService;
        private readonly IShippingChangesService _shippingChangesService;

        public ValidateClearBacklightFlags(
            IUserProvider userProvider,
            ICommonDataService dataService,
            IShippingChangesService shippingChangesService)
        {
            _userProvider = userProvider;
            _dataService = dataService;
            _shippingChangesService = shippingChangesService;
        }

        public ValidateResult Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            var user = _userProvider.GetCurrentUser();
            var role = user?.RoleId == null ? null : _dataService.GetById<Role>(user.RoleId.Value);
            var entities = changes.Select(x => x.Entity).ToList();
            _shippingChangesService.ClearBacklightFlags(entities, role);
            return new ValidateResult();
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes;
        }
    }
}
