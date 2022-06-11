using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.OrderStates;
using Domain.Shared.UserProvider;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.OrderStates
{
    public class OrderStateService : IOrderStateService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;

        public OrderStateService(ICommonDataService dataService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _userProvider = userProvider;
        }

        public IEnumerable<StateDto> GetAll()
        {
            var values = GetValues();
            var result = new List<StateDto>();
            foreach (var value in values)
            {
                string name = value.FormatEnum();
                result.Add(new StateDto
                {
                    Name = name,
                    Value = name,
                    Color = value.GetColor().FormatEnum()
                });
            }
            return result;
        }

        public IEnumerable<LookUpDto> ForSelect()
        {
            var values = GetValues();
            var result = new List<LookUpDto>();
            foreach (var value in values)
            {
                result.Add(new LookUpDto
                {
                    Name = value.FormatEnum(),
                    Value = value.ToString()
                });
            }
            return result;
        }

        private IEnumerable<OrderState> GetValues()
        {
            var result = Domain.Extensions.Extensions.GetOrderedEnum<OrderState>();

            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            var company = companyId == null ? null : _dataService.GetById<Company>(companyId.Value);
            if (company != null && company.OrderRequiresConfirmation != true)
            {
                result = result.Except(new[] { OrderState.Confirmed });
            }

            return result;
        }
    }
}
