using Domain.Extensions;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Application.Extensions
{
    /// <summary>
    /// Filter Extentions
    /// </summary>
    public static class FiltersExtentions
    {
        /// <summary>
        /// Apply numeric filter (float)
        /// </summary>
        public static string ApplyNumericFilter<TModel>(this string search, Expression<Func<TModel, decimal?>> property, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            var precision = GetPrecision(search);

            decimal? searchValue = search.ToDecimal();
            if (searchValue == null) return string.Empty;

            int paramInd = parameters.Count();
            parameters.Add(searchValue.Value);
            return $@"ROUND(""{fieldName}"",{precision}) = ROUND({{{paramInd}}},{precision})";
        }

        private static int GetPrecision(string number)
        {
            var dotPos = number.IndexOf('.');
            return dotPos > 0 ? number.Length - dotPos - 1 : 0;
        }

        public static string ApplyBoolenFilter<TModel>(this string search, Expression<Func<TModel, bool>> property, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            bool? searchValue = search.ToBool();
            if (searchValue == null) return string.Empty;

            int paramInd = parameters.Count();
            parameters.Add(searchValue.Value);
            return $@"""{fieldName}"" = {{{paramInd}}}";
        }

        /// <summary>
        /// Apply numeric filter (integer)
        /// </summary>
        public static string ApplyNumericFilter<TModel>(this string search, Expression<Func<TModel, int?>> property, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            int? searchValue = search.ToInt();
            if (searchValue == null) return string.Empty;

            int paramInd = parameters.Count();
            parameters.Add(searchValue.Value);
            return $@"""{fieldName}"" = {{{paramInd}}}";
        }

        /// <summary>
        /// Apply boolean filter
        /// </summary>
        public static string ApplyBooleanFilter<TModel>(this string search, Expression<Func<TModel, bool>> property, ref List<object> parameters)
        {
            string fieldName = property.GetPropertyName();
            return search.ApplyBooleanFilterBase(fieldName, ref parameters);
        }

        /// <summary>
        /// Apply boolean filter
        /// </summary>
        public static string ApplyBooleanFilter<TModel>(this string search, Expression<Func<TModel, bool?>> property, ref List<object> parameters)
        {
            string fieldName = property.GetPropertyName();
            return search.ApplyBooleanFilterBase(fieldName, ref parameters);
        }

        private static string ApplyBooleanFilterBase(this string search, string fieldName, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            StringBuilder condition = new StringBuilder();
            var values = search.ToLower().Split('|');
            foreach (var value in values)
            {
                if (value == "null")
                {
                    if (condition.Length > 0)
                    {
                        condition.Append(" or ");
                    }
                    condition.Append($@"""{fieldName}"" is null");
                }
                else if (bool.TryParse(value, out bool parsedValue))
                {
                    if (condition.Length > 0)
                    {
                        condition.Append(" or ");
                    }
                    int paramInd = parameters.Count();
                    parameters.Add(parsedValue);
                    condition.Append($@"""{fieldName}"" = {{{paramInd}}}");
                }
            }

            if (condition.Length > 0)
            {
                return $"({condition})";
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Apply date range filter
        /// </summary>
        public static string ApplyDateRangeFilter<TModel>(this string search, Expression<Func<TModel, DateTime?>> property, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            var dates = search.Split("-");

            var fromDateStr = dates.FirstOrDefault();
            var toDateStr = dates.ElementAtOrDefault(1);

            if (!search.Contains('-'))
            {
                toDateStr = fromDateStr;
            }

            StringBuilder result = new StringBuilder();

            DateTime? fromDate = fromDateStr.ToDate();
            if (fromDate != null)
            {
                int paramInd = parameters.Count();
                parameters.Add(fromDate.Value);
                result.Append($@"""{fieldName}"" >= {{{paramInd}}}");
            }

            DateTime? toDate = toDateStr.ToDate();
            if (toDate != null)
            {
                if (result.Length > 0)
                {
                    result.Append(" AND ");
                }
                int paramInd = parameters.Count();
                parameters.Add(toDate.Value.AddDays(1));
                result.Append($@"""{fieldName}"" < {{{paramInd}}}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Apply time range filter
        /// </summary>
        public static string ApplyTimeRangeFilter<TModel>(this string search, Expression<Func<TModel, TimeSpan?>> property, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            string field = $@"""{fieldName}""";
            return search.ApplyTimeRangeFilterBase<TModel>(field, ref parameters);
        }

        /// <summary>
        /// Apply time range filter
        /// </summary>
        public static string ApplyTimeRangeFilter<TModel>(this string search, Expression<Func<TModel, DateTime?>> property, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            string field = $@"to_char(""{fieldName}"", 'HH24:MI:SS')::time";
            return search.ApplyTimeRangeFilterBase<TModel>(field, ref parameters);
        }

        /// <summary>
        /// Apply string filter
        /// </summary>
        public static string ApplyStringFilter<TModel>(this string search, Expression<Func<TModel, string>> property, ref List<object> parameters)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            int paramInd = parameters.Count();
            parameters.Add($"%{search}%");
            return $@"""{fieldName}"" ~~* {{{paramInd}}}";
        }

        /// <summary>
        /// Apply options filter
        /// </summary>
        public static string ApplyOptionsFilter<TModel, TValue>(this string search, Expression<Func<TModel, TValue>> property, ref List<object> parameters)
        {
            return search.ApplyOptionsFilterBase(property, ref parameters, x => x);
        }

        /// <summary>
        /// Apply options filter
        /// </summary>
        public static string ApplyOptionsFilter<TModel, TValue>(this string search, Expression<Func<TModel, TValue>> property, ref List<object> parameters, Func<string, object> selection)
        {
            return search.ApplyOptionsFilterBase(property, ref parameters, selection);
        }

        /// <summary>
        /// Apply options filter
        /// </summary>
        public static string ApplyOptionsArrayFilter<TModel, TValue>(this string search, Expression<Func<TModel, TValue>> property, ref List<object> parameters, Func<string, object> selection)
        {
            return search.ApplyOptionsArrayFilterBase(property, ref parameters, selection);
        }

        /// <summary>
        /// Apply options filter
        /// </summary>
        public static string ApplyEnumArrayFilter<TModel, TValue, TEnum>(this string search, Expression<Func<TModel, TValue>> property, ref List<object> parameters)
            where TEnum : struct, Enum
        {
            return search.ApplyOptionsArrayFilterBase(property, ref parameters, x => MapFromStateDto<TEnum>(x));
        }

        /// <summary>
        /// Apply options filter
        /// </summary>
        public static string ApplyEnumFilter<TModel, TEnum>(this string search, Expression<Func<TModel, TEnum>> property, ref List<object> parameters) 
            where TEnum : struct, Enum
        {
            return search.ApplyOptionsFilterBase(property, ref parameters, x => MapFromStateDto<TEnum>(x));
        }

        /// <summary>
        /// Apply options filter
        /// </summary>
        public static string ApplyEnumFilter<TModel, TEnum>(this string search, Expression<Func<TModel, TEnum?>> property, ref List<object> parameters)
            where TEnum : struct, Enum
        {
            return search.ApplyOptionsFilterBase(property, ref parameters, x => MapFromStateDto<TEnum>(x));
        }

        /// <summary>
        /// Add WHERE condition with AND operator
        /// </summary>
        public static string WhereAnd(this string where, string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return where;
            }
            else if (string.IsNullOrEmpty(where))
            {
                return $"WHERE {condition}";
            }
            else
            {
                return $"{where} AND {condition}";
            }
        }

        public static IQueryable<TModel> OrderBy<TModel>(this IQueryable<TModel> query, string propertyName, bool? descending)
        {
            if (string.IsNullOrEmpty(propertyName)) return query;

            var propertyInfo = typeof(TModel).GetProperties()
                .FirstOrDefault(i => i.Name.ToLower() == propertyName.ToLower());

            if (propertyInfo == null) return query;

            ParameterExpression param = Expression.Parameter(typeof(TModel), string.Empty);
            Expression property = Expression.PropertyOrField(param, propertyInfo.Name);

            var isNavigationProperty = propertyName.EndsWith("Id");

            if (propertyInfo.PropertyType == typeof(string))
            {
                var trimMethod = propertyInfo.PropertyType.GetMethod("Trim", new Type[0]);
                property = Expression.Call(property, trimMethod);
            }
            else if (isNavigationProperty)
            {
                var navigationPropertyName = propertyName.Replace("Id", "").ToUpperFirstLetter();
                var navigationPropertyInfo = typeof(TModel).GetProperties()
                    .FirstOrDefault(i => i.Name.ToLower() == navigationPropertyName.ToLower());

                property = Expression.PropertyOrField(param, navigationPropertyName);
                var sortAttribute = (SortKeyAttribute)navigationPropertyInfo.GetCustomAttributes().FirstOrDefault(i => i.GetType() == typeof(SortKeyAttribute));

                property = Expression.PropertyOrField(property, sortAttribute.SortKey);
            } 

            LambdaExpression sort = Expression.Lambda(property, param);
            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                 "OrderBy" + (descending.GetValueOrDefault() ? "Descending" : string.Empty),
                new[] { typeof(TModel), property.Type },
                query.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<TModel>)query.Provider.CreateQuery<TModel>(call);
        }

        public static IQueryable<Guid?> SelectField<TModel>(this IQueryable<TModel> query, string propertyName)
        {
            var propertyInfo = typeof(TModel).GetProperties()
                .FirstOrDefault(i => i.Name.ToLower() == propertyName.ToLower());

            ParameterExpression param = Expression.Parameter(typeof(TModel), string.Empty);
            MemberExpression property = Expression.PropertyOrField(param, propertyInfo.Name);

            Expression<Func<TModel, Guid?>> select;
            if (property.Type == typeof(Guid))
            {
                var convert = Expression.Convert(property, typeof(Guid?));
                select = Expression.Lambda<Func<TModel, Guid?>>(convert, param);
            }
            else
            {
                select = Expression.Lambda<Func<TModel, Guid?>>(property, param);
            }

            return query.Select(select);
        }

        public static IQueryable<TSource> DefaultOrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool secondarySort, bool desc = false)
        {
            IOrderedQueryable<TSource> ordered = source as IOrderedQueryable<TSource>;
            bool isOrdered = ordered != null && typeof(IOrderedQueryable<TSource>).IsAssignableFrom(ordered.Expression.Type);

            if (secondarySort && isOrdered)
            {
                return desc ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
            }
            else
            {
                return desc ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
            }
        }

        private static TEnum MapFromStateDto<TEnum>(string dtoStatus) where TEnum : struct, Enum
        {
            var mapFromStateDto = dtoStatus.ToEnum<TEnum>() ?? default;
            return mapFromStateDto;
        }

        public static string GetPropertyName<TModel, TValue>(this Expression<Func<TModel, TValue>> property)
        {
            var propertyBody = property?.Body as MemberExpression;
            if (propertyBody != null)
            {
                var propertyInfo = propertyBody.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    return propertyInfo.Name;
                }
            }
            return null;
        }

        public static string ApplyOptionsFilterBase<TModel, TValue>(this string search,
            Expression<Func<TModel, TValue>> property,
            ref List<object> parameters,
            Func<string, object> parameterValueLookup)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            var values = search.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList();

            string emptyFilterPart = null;
            if (values.Contains(LookUpDto.EmptyValue))
            {
                emptyFilterPart = $@"""{fieldName}"" is null";
            }

            string inFilterPart = null;
            var nonEmptyValues = values.Where(x => x != LookUpDto.EmptyValue);
            if (nonEmptyValues.Any())
            {
                StringBuilder inValue = new StringBuilder();
                foreach (string value in nonEmptyValues)
                {
                    if (inValue.Length > 0)
                    {
                        inValue.Append(',');
                    }
                    int paramInd = parameters.Count();
                    parameters.Add(parameterValueLookup(value));
                    inValue.Append($"{{{paramInd}}}");
                }
                inFilterPart = $@"""{fieldName}"" in ({inValue})";
            }

            if (!string.IsNullOrEmpty(emptyFilterPart) && !string.IsNullOrEmpty(inFilterPart))
            {
                return $"({emptyFilterPart} or {inFilterPart})";
            }
            else
            {
                return emptyFilterPart ?? inFilterPart ?? string.Empty;
            }
        }

        public static string ApplyOptionsArrayFilterBase<TModel, TValue>(this string search,
            Expression<Func<TModel, TValue>> property,
            ref List<object> parameters,
            Func<string, object> parameterValueLookup)
        {
            if (string.IsNullOrEmpty(search)) return string.Empty;

            string fieldName = property.GetPropertyName();
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;

            var values = search.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList();
            if (values.Any())
            {
                StringBuilder inValue = new StringBuilder();
                foreach (string value in values)
                {
                    if (inValue.Length > 0)
                    {
                        inValue.Append(',');
                    }
                    int paramInd = parameters.Count();
                    parameters.Add(parameterValueLookup(value));
                    inValue.Append($"{{{paramInd}}}");
                }
                return $@"""{fieldName}"" && ARRAY[{inValue}]";
            }
            else
            {
                return string.Empty;
            }
        }

        private static string ApplyTimeRangeFilterBase<TModel>(this string search, string field, ref List<object> parameters)
        {
            var times = search.Split("-");

            var fromTimeStr = times.FirstOrDefault();
            var toTimeStr = times.ElementAtOrDefault(1);

            StringBuilder result = new StringBuilder();

            TimeSpan? fromTime = fromTimeStr.ToTime();
            if (fromTime != null)
            {
                int paramInd = parameters.Count();
                parameters.Add(fromTime.Value);
                result.Append($@"{field} >= {{{paramInd}}}");
            }

            TimeSpan? toTime = toTimeStr.ToTime();
            if (toTime != null)
            {
                if (result.Length > 0)
                {
                    result.Append(" AND ");
                }
                int paramInd = parameters.Count();
                parameters.Add(toTime.Value);
                result.Append($@"{field} <= {{{paramInd}}}");
            }

            return result.ToString();
        }
    }
}
