using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared.UserProvider;
using Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.OrderItems.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncItemProductData : ITrigger<OrderItem>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly IUserProvider _userProvider;

        public SyncItemProductData(ICommonDataService dataService, IHistoryService historyService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _historyService = historyService;
            _userProvider = userProvider;
        }

        public void Execute(IEnumerable<EntityChanges<OrderItem>> changes)
        {
            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            var narts = changes.Select(x => x.Entity.Nart).Distinct().ToList();
            var productsDict = _dataService.GetDbSet<Article>()
                                           .Where(x => x.CompanyId == companyId && narts.Contains(x.Nart))
                                           .ToDictionary(x => x.Nart);

            foreach (var change in changes)
            {
                var entity = change.Entity;

                if (!string.IsNullOrEmpty(entity.Nart) 
                    && productsDict.TryGetValue(entity.Nart, out Article product))
                {
                    entity.Description = product.Description;
                }

                if (change.Status != EntityStatus.Added)
                {
                    var fieldChange = change.FieldChanges.FirstOrDefault(x => x.FieldName == nameof(OrderItem.Nart));
                    _historyService.Save(entity.OrderId, "orderItemChangeNart", fieldChange.OldValue, fieldChange.NewValue);
                }
            }
        }

        public IEnumerable<EntityChanges<OrderItem>> FilterTriggered(IEnumerable<EntityChanges<OrderItem>> changes)
        {
            return changes.FilterChanged(x => x.Nart);
        }
    }
}
