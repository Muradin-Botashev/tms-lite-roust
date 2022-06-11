using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Articles.Triggers
{
    [TriggerCategory(TriggerCategory.Synchronization)]
    public class SyncArticleFields : ITrigger<Article>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;

        public SyncArticleFields(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
        }

        public void Execute(IEnumerable<EntityChanges<Article>> changes)
        {
            foreach (var entityGroup in changes.Select(x => x.Entity).GroupBy(x => x.CompanyId))
            {
                var articleNarts = entityGroup.Select(x => x.Nart).ToList();
                var articlesDict = entityGroup.ToDictionary(x => x.Nart);

                var validStatuses = new[] { OrderState.Draft, OrderState.Created, OrderState.Confirmed, OrderState.InShipping,
                                            OrderState.Shipped, OrderState.Delivered };
                var items = _dataService.GetDbSet<OrderItem>()
                                        .Include(x => x.Order)
                                        .Where(x => x.Nart != null
                                                    && articleNarts.Contains(x.Nart)
                                                    && x.Order != null
                                                    && x.Order.CompanyId == entityGroup.Key
                                                    && validStatuses.Contains(x.Order.Status))
                                        .ToList();

                foreach (var orderItem in items)
                {
                    var entity = articlesDict[orderItem.Nart];

                    if (entity.Description != orderItem.Description)
                    {
                        _historyService.SaveImpersonated(null, orderItem.OrderId,
                                                         "orderItemChangeDescription",
                                                         orderItem.Nart, entity.Description);
                        orderItem.Description = entity.Description;
                    }
                }
            }
        }

        public IEnumerable<EntityChanges<Article>> FilterTriggered(IEnumerable<EntityChanges<Article>> changes)
        {
            return changes.FilterChanged(
                x => x.Nart,
                x => x.Description,
                x => x.TemperatureRegime);
        }
    }
}