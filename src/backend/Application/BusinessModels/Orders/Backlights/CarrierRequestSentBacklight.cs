using Application.BusinessModels.Shared.Backlights;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using System.Linq;

namespace Application.BusinessModels.Orders.Backlights
{
    public class CarrierRequestSentBacklight : IBacklight<Order>
    {
        private readonly IUserProvider _userProvider;
        private readonly ICommonDataService _dataService;

        public CarrierRequestSentBacklight(IUserProvider userProvider, ICommonDataService dataService)
        {
            _userProvider = userProvider;
            _dataService = dataService;
        }

        public BacklightType Type => BacklightType.CarrierRequestSentBacklight;

        public bool IsActive(Order entity)
        {
            var roleId = _userProvider.GetCurrentUser()?.RoleId;
            if (roleId.HasValue)
            {
                var role = _dataService.GetById<Role>(roleId.Value);
                return entity.IsNewCarrierRequest && role?.Backlights?.Contains((int)Type) == true;
            }
            return false;
        }
    }
}
