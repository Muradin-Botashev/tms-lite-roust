using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Application.BusinessModels.Shared.Triggers
{
    public static class TriggerExtensions
    {
        public static bool IsChanged<TEntity>(this EntityChanges<TEntity> changes, params string[] fieldNames)
            where TEntity : class, IPersistable
        {
            return changes?.FieldChanges.Count(x => fieldNames.Select(y => y.ToLower()).Contains(x.FieldName.ToLower())) > 0;
        }

        public static bool IsChanged<TEntity>(this EntityChanges<TEntity> changes, params Expression<Func<TEntity, object>>[] properties)
            where TEntity : class, IPersistable
        {
            var fieldNames = GetFieldNames(properties);
            return changes.IsChanged(fieldNames.ToArray());
        }

        public static bool IsManuallyChanged<TEntity>(this EntityChanges<TEntity> changes, params Expression<Func<TEntity, object>>[] properties)
            where TEntity : class, IPersistable
        {
            var fieldNames = GetFieldNames(properties);
            return changes?.FieldChanges.Count(x => x.IsManual && fieldNames.Select(y => y.ToLower()).Contains(x.FieldName.ToLower())) > 0;
        }

        public static IEnumerable<EntityChanges<TEntity>> FilterChanged<TEntity>(
            this IEnumerable<EntityChanges<TEntity>> changes,
            params string[] fieldNames)
            where TEntity : class, IPersistable
        {
            return changes?.Where(c => c.IsChanged(fieldNames));
        }

        public static IEnumerable<EntityChanges<TEntity>> FilterChanged<TEntity>(
            this IEnumerable<EntityChanges<TEntity>> changes,
            params Expression<Func<TEntity, object>>[] properties)
            where TEntity : class, IPersistable
        {
            return changes?.Where(c => c.IsChanged(properties));
        }

        private static List<string> GetFieldNames<TEntity>(IEnumerable<Expression<Func<TEntity, object>>> properties)
            where TEntity : class, IPersistable
        {
            var fieldNames = new List<string>();
            foreach (var property in properties)
            {
                var propertyBody = property.Body as MemberExpression;

                if (propertyBody == null)
                {
                    propertyBody = (property.Body as UnaryExpression)?.Operand as MemberExpression;
                }

                if (propertyBody != null)
                {
                    fieldNames.Add(propertyBody.Member.Name.ToLower());
                }
            }
            return fieldNames;
        }
    }
}
