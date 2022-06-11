using Application.BusinessModels.Shared.Triggers;
using DAL.Services;
using Domain.Persistables;
using Domain.Services;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Application.Shared.Triggers
{
    public class TriggersService : ITriggersService
    {
        private readonly ICommonDataService _dataService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChangeTrackerFactory _changeTrackerFactory;

        public TriggersService(
            ICommonDataService dataService, 
            IServiceProvider serviceProvider,
            IChangeTrackerFactory changeTrackerFactory)
        {
            _dataService = dataService;
            _serviceProvider = serviceProvider;
            _changeTrackerFactory = changeTrackerFactory;
        }

        public ValidateResult Execute(bool isManual)
        {
            var dbChanges = _dataService.GetChanges(isManual).ToList();

            var validationResult = ExecuteValidation(dbChanges);
            if (validationResult.IsError)
            {
                return validationResult;
            }

            var triggerCategories = Enum.GetValues(typeof(TriggerCategory))
                                        .Cast<TriggerCategory>()
                                        .OrderBy(x => x)
                                        .ToList();

            foreach (var triggerCategory in triggerCategories)
            {
                MergeChanges(ref dbChanges, _dataService.GetChanges(false));
                Execute(triggerCategory, dbChanges);
            }

            MergeChanges(ref dbChanges, _dataService.GetChanges(false));
            if (dbChanges != null)
            {
                _changeTrackerFactory
                    .CreateChangeTracker()
                    .TrackAll<Order>()
                    .TrackAll<Shipping>()
                    .LogTrackedChanges(dbChanges);
            }

            return validationResult;
        }

        private ValidateResult ExecuteValidation(IEnumerable<EntityChanges> changes)
        {
            var result = new ValidateResult();

            if (changes == null || !changes.Any()) return result;

            var articleChanges = changes.Where(i => i.Entity.GetType() == typeof(Article)).Select(i => i.As<Article>());
            var orderChanges = changes.Where(i => i.Entity.GetType() == typeof(Order)).Select(i => i.As<Order>());
            var orderItemChanges = changes.Where(i => i.Entity.GetType() == typeof(OrderItem)).Select(i => i.As<OrderItem>());
            var shippingChanges = changes.Where(i => i.Entity.GetType() == typeof(Shipping)).Select(i => i.As<Shipping>());
            var shippingWarehouseChanges = changes.Where(i => i.Entity.GetType() == typeof(ShippingWarehouse)).Select(i => i.As<ShippingWarehouse>());
            var tariffChanges = changes.Where(i => i.Entity.GetType() == typeof(Tariff)).Select(i => i.As<Tariff>());
            var warehouseChanges = changes.Where(i => i.Entity.GetType() == typeof(Warehouse)).Select(i => i.As<Warehouse>());

            result = JoinValidateResults(result,
                RunValidationTriggers(articleChanges),
                RunValidationTriggers(orderChanges),
                RunValidationTriggers(orderItemChanges),
                RunValidationTriggers(shippingChanges),
                RunValidationTriggers(shippingWarehouseChanges),
                RunValidationTriggers(tariffChanges),
                RunValidationTriggers(warehouseChanges)
            );

            return result;
        }

        private ValidateResult RunValidationTriggers<TEntity>(IEnumerable<EntityChanges<TEntity>> changes)
            where TEntity : class, IPersistable
        {
            var result = new ValidateResult();

            if (changes == null || !changes.Any()) return result;

            var triggers = _serviceProvider.GetService<IEnumerable<IValidationTrigger<TEntity>>>().ToList();
            if (triggers.Any())
            {
                foreach (var trigger in triggers)
                {
                    var triggeredChanges = trigger.FilterTriggered(changes);
                    if (triggeredChanges != null && triggeredChanges.Any())
                    {
                        var triggerResult = trigger.Execute(triggeredChanges);
                        result = JoinValidateResults(triggerResult, result);
                    }
                }
            }

            return result;
        }

        private ValidateResult JoinValidateResults(ValidateResult source, ValidateResult target)
        {
            if (source == null || (target != null && !source.IsError))
            {
                return target;
            }

            if (target == null || (source != null && !target.IsError))
            {
                return source;
            }

            return new ValidateResult(string.Join(' ', target.Message, source.Message));
        }

        private ValidateResult JoinValidateResults(ValidateResult target, params ValidateResult[] sources)
        {
            var result = target;
            foreach (var source in sources)
            {
                result = JoinValidateResults(source, target);
            }
            return result;
        }

        private void Execute(TriggerCategory triggerCategory, IEnumerable<EntityChanges> changes)
        {
            if (changes == null || !changes.Any()) return;

            var articleChanges = changes.Where(i => i.Entity.GetType() == typeof(Article)).Select(i => i.As<Article>());
            var orderChanges = changes.Where(i => i.Entity.GetType() == typeof(Order)).Select(i => i.As<Order>());
            var orderItemChanges = changes.Where(i => i.Entity.GetType() == typeof(OrderItem)).Select(i => i.As<OrderItem>());
            var shippingChanges = changes.Where(i => i.Entity.GetType() == typeof(Shipping)).Select(i => i.As<Shipping>());
            var shippingWarehouseChanges = changes.Where(i => i.Entity.GetType() == typeof(ShippingWarehouse)).Select(i => i.As<ShippingWarehouse>());
            var tariffChanges = changes.Where(i => i.Entity.GetType() == typeof(Tariff)).Select(i => i.As<Tariff>());
            var warehouseChanges = changes.Where(i => i.Entity.GetType() == typeof(Warehouse)).Select(i => i.As<Warehouse>());

            _dataService.SaveChanges();

            RunTriggers(triggerCategory, articleChanges);
            RunTriggers(triggerCategory, orderChanges);
            RunTriggers(triggerCategory, orderItemChanges);
            RunTriggers(triggerCategory, shippingChanges);
            RunTriggers(triggerCategory, shippingWarehouseChanges);
            RunTriggers(triggerCategory, tariffChanges);
            RunTriggers(triggerCategory, warehouseChanges);
        }

        private void RunTriggers<TEntity>(TriggerCategory triggerCategory, IEnumerable<EntityChanges<TEntity>> changes) 
            where TEntity : class, IPersistable
        {
            if (changes == null || !changes.Any()) return;

            var triggers = _serviceProvider.GetService<IEnumerable<ITrigger<TEntity>>>()
                                           .Where(x => IsProperCategory(x, triggerCategory))
                                           .ToList();
            if (triggers.Any())
            {
                foreach (var trigger in triggers)
                {
                    var triggeredChanges = trigger.FilterTriggered(changes);
                    if (triggeredChanges != null && triggeredChanges.Any())
                    {
                        trigger.Execute(triggeredChanges);
                    }
                }
            }
        }

        private bool IsProperCategory<TEntity>(ITrigger<TEntity> trigger, TriggerCategory triggerCategory)
            where TEntity : class, IPersistable
        {
            var categoryAttribute = trigger.GetType().GetCustomAttribute<TriggerCategoryAttribute>();
            return categoryAttribute?.Category == triggerCategory;
        }

        private void MergeChanges(ref List<EntityChanges> originChanges, IEnumerable<EntityChanges> newChanges)
        {
            if (!newChanges.Any()) return;

            var originChangesDict = originChanges.ToDictionary(x => x.Entity.Id); 
            foreach (var change in newChanges)
            {
                if (change.Status == EntityStatus.Deleted)
                {
                    if (originChangesDict.TryGetValue(change.Entity.Id, out EntityChanges originChange))
                    {
                        originChanges.Remove(originChange);
                        originChangesDict.Remove(change.Entity.Id);
                    }
                }
                else if (originChangesDict.TryGetValue(change.Entity.Id, out EntityChanges originChange))
                {
                    var originFieldChanges = originChange.FieldChanges.ToDictionary(x => x.FieldName);
                    foreach (var fieldChange in change.FieldChanges)
                    {
                        if (originFieldChanges.TryGetValue(fieldChange.FieldName, out EntityFieldChanges originFieldChange))
                        {
                            originFieldChange.NewValue = fieldChange.NewValue;
                        }
                        else
                        {
                            originChange.FieldChanges.Add(fieldChange);
                        }
                    }
                }
                else
                {
                    originChanges.Add(change);
                }
            }
        }
    }
}
