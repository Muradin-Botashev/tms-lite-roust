using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Shared
{
    public class ChangeTracker : IChangeTracker
    {
        private readonly ICommonDataService _dataService;

        private readonly IHistoryService _historyService;

        Dictionary<string, EntityTrackerConfiguration> TypeConfigurations { get; set; } = new Dictionary<string, EntityTrackerConfiguration>();

        public ChangeTracker(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
        }

        public IChangeTracker Add<TEntity>(Expression<Func<TEntity, object>> property, string formatString = null)
        {
            var typeName = typeof(TEntity).Name;
            var prop = this.GetProperty(property);

            Add(typeName, new PropertyTrackerConfiguration
            {
                Property = prop,
                FormatString = formatString
            });

            return this;
        }

        private object FormatValue(PropertyTrackerConfiguration property, object value)
        {
            if (value == null) return value;

            if (value is TimeSpan)
            {
                if (string.IsNullOrEmpty(property.FormatString))
                {
                    return ((TimeSpan)value).FormatTime();
                }
                else
                {
                    return ((TimeSpan)value).ToString(property.FormatString);
                }
            }
            else if (value is DateTime)
            {
                if (string.IsNullOrEmpty(property.FormatString))
                {
                    return ((DateTime)value).FormatDateTime();
                }
                else
                {
                    return ((DateTime)value).ToString(property.FormatString);
                }
            }
            else if (value is decimal)
            {
                return Math.Round((decimal)value, 2).ToString("0.##");
            }
            else
            {
                return value;
            }
        }

        private void Add(string typeName, PropertyTrackerConfiguration property)
        { 
            var config = GetTypeConfiguration(typeName);
            config.Properties[property.Property.Name] = property;
        }

        public IChangeTracker Remove<TEntity>(Expression<Func<TEntity, object>> property)
        {
            var typeName = typeof(TEntity).Name;

            var config = GetTypeConfiguration(typeName);

            var prop = this.GetProperty(property);

            config.Properties.Remove(prop.Name);

            return this;
        }

        public IChangeTracker TrackAll<TEntity>()
        {
            var properties = typeof(TEntity).GetProperties();
            var typeName = typeof(TEntity).Name;

            foreach (var prop in properties)
            {
                if (!prop.GetCustomAttributes<IgnoreHistoryAttribute>().Any())
                {
                    Add(typeName, new PropertyTrackerConfiguration 
                    {
                        Property = prop
                    });
                }
            }

            return this;
        }

        public void LogTrackedChanges<TEntity>(EntityChanges<TEntity> change) where TEntity : class, IPersistable
        {
            LogTrackedChanges(new EntityChanges
            { 
                Entity = change.Entity,
                Status = change.Status,
                FieldChanges = change.FieldChanges
            });
        }

        public void LogTrackedChanges<TEntity>(IEnumerable<EntityChanges<TEntity>> changes) where TEntity : class, IPersistable
        {
            if (changes != null)
            {
                foreach (var change in changes)
                {
                    LogTrackedChanges(change);
                }
            }
        }

        public void LogTrackedChanges(EntityChanges change)
        {
            if (change?.FieldChanges == null || change?.Status == EntityStatus.Deleted) return;

            var config = GetTypeConfiguration(change.Entity.GetType().Name);

            var changes = change.FieldChanges.Where(f => config.Properties.ContainsKey(f.FieldName)).ToList();

            foreach (var field in changes)
            {
                var property = config.Properties[field.FieldName];

                object newValue = field.NewValue;
                if (newValue != null && (property?.Property?.PropertyType == typeof(Guid) || property?.Property?.PropertyType == typeof(Guid?)))
                {
                    newValue = LoadReferenceName(field, property.Property) ?? newValue;
                }

                _historyService.Save(change.Entity.Id, "fieldChanged",
                                        field.FieldName.ToLowerFirstLetter(),
                                        FormatValue(property, field.OldValue), 
                                        FormatValue(property, newValue));
            }
        }

        public void LogTrackedChanges(IEnumerable<EntityChanges> changes)
        {
            changes.ToList().ForEach(LogTrackedChanges);
        }

        public void LogTrackedChanges()
        {
            var changes = _dataService.GetChanges();

            foreach (var change in changes)
            {
                LogTrackedChanges(change);
            }
        }

        private EntityTrackerConfiguration GetTypeConfiguration(string typeName)
        {
            if (!TypeConfigurations.ContainsKey(typeName))
            {
                TypeConfigurations.Add(typeName, new EntityTrackerConfiguration { TypeName = typeName });
            }

            return TypeConfigurations[typeName];
        }

        private Type GetReferenceType(PropertyInfo property)
        {
            var attr = property.GetCustomAttribute<ReferenceTypeAttribute>();
            return attr?.Type;
        }

        private object LoadReferenceName(EntityFieldChanges field, PropertyInfo property)
        {
            Type refType = GetReferenceType(property);
            if (refType != null)
            {
                object refId = field.NewValue;
                if (property.PropertyType == typeof(Guid?))
                {
                    refId = ((Guid?)refId).Value;
                }
                var getMethod = _dataService.GetType().GetMethod(nameof(_dataService.GetById)).MakeGenericMethod(refType);
                var refEntity = getMethod.Invoke(_dataService, new[] { refId });
                return refEntity?.ToString();
            }
            return null;
        }

        private PropertyInfo GetProperty<TEntity, T>(Expression<Func<TEntity, T>> property)
        {
            var castExp = property.Body as UnaryExpression;

            var propertyBody = (castExp != null ? castExp.Operand : property.Body) as MemberExpression;
            
            if (propertyBody == null) return null;

            var propertyInfo = propertyBody.Member as PropertyInfo;

            return propertyInfo;
        }
        private class EntityTrackerConfiguration
        { 
            public string TypeName { get; set; }

            public Dictionary<string, PropertyTrackerConfiguration> Properties { get; set; } = new Dictionary<string, PropertyTrackerConfiguration>();
        }

        private class PropertyTrackerConfiguration
        {
            public PropertyInfo Property { get; set; }

            public string FormatString { get; set; }
        }
    }
}
