using Domain.Extensions;
using Domain.Services.FieldProperties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FieldProperties
{
    public class FieldDispatcherService : IFieldDispatcherService
    {
        private readonly Dictionary<Type, IEnumerable<FieldInfo>> _fieldsCache;

        public FieldDispatcherService()
        {
            _fieldsCache = new Dictionary<Type, IEnumerable<FieldInfo>>();
        }

        public IEnumerable<FieldInfo> GetDtoFields<TDto>()
        {
            Type dtoType = typeof(TDto);
            IEnumerable<FieldInfo> result;
            if (!_fieldsCache.TryGetValue(dtoType, out result))
            {
                result = GetDtoFieldsInner<TDto>();
                _fieldsCache[dtoType] = result;
            }
            return result;
        }

        private IEnumerable<FieldInfo> GetDtoFieldsInner<TDto>()
        {
            var props = typeof(TDto).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FieldTypeAttribute)));
            foreach (var prop in props)
            {
                var fieldTypeAttr = (FieldTypeAttribute)Attribute.GetCustomAttribute(prop, typeof(FieldTypeAttribute));

                bool isDefault = Attribute.IsDefined(prop, typeof(IsDefaultAttribute));
                bool isBulkUpdateAllowed = Attribute.IsDefined(prop, typeof(AllowBulkUpdateAttribute));
                bool isFixedPosition = Attribute.IsDefined(prop, typeof(IsFixedPositionAttribute));
                bool isRequired = Attribute.IsDefined(prop, typeof(IsRequiredAttribute));
                bool isReadOnly = Attribute.IsDefined(prop, typeof(IsReadOnlyAttribute));
                bool isSortDisabled = Attribute.IsDefined(prop, typeof(IsSortDisabledAttribute));

                int orderNumber = int.MaxValue;
                if (Attribute.IsDefined(prop, typeof(OrderNumberAttribute)))
                {
                    var orderNumberAttr = (OrderNumberAttribute)Attribute.GetCustomAttribute(prop, typeof(OrderNumberAttribute));
                    orderNumber = orderNumberAttr.Value;
                }

                string displayNameKey = prop.Name.ToLowerFirstLetter();
                if (Attribute.IsDefined(prop, typeof(DisplayNameKeyAttribute)))
                {
                    var keyAttr = (DisplayNameKeyAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameKeyAttribute));
                    displayNameKey = keyAttr.Key;
                }

                int? decimals = null;
                if (Attribute.IsDefined(prop, typeof(RoundAttribute)))
                {
                    var roundAttr = (RoundAttribute)Attribute.GetCustomAttribute(prop, typeof(RoundAttribute));
                    decimals = roundAttr.Decimals;
                }

                int? maxLength = null;
                if (Attribute.IsDefined(prop, typeof(MaxLengthAttribute)))
                {
                    var maxLengthAttr = (MaxLengthAttribute)Attribute.GetCustomAttribute(prop, typeof(MaxLengthAttribute));
                    maxLength = maxLengthAttr.Length;
                }

                var fieldInfo = new FieldInfo
                {
                    Name = prop.Name,
                    DisplayNameKey = displayNameKey,
                    FieldType = fieldTypeAttr.Type,
                    FilterType = fieldTypeAttr.FilterType,
                    ReferenceSource = fieldTypeAttr.Source,
                    ShowRawReferenceValue = fieldTypeAttr.ShowRawValue,
                    OrderNumber = orderNumber,
                    Decimals = decimals,
                    IsDefault = isDefault,
                    IsBulkUpdateAllowed = isBulkUpdateAllowed,
                    IsFixedPosition = isFixedPosition,
                    IsRequired = isRequired,
                    IsReadOnly = isReadOnly,
                    IsSortDisabled = isSortDisabled,
                    MaxLength = maxLength,
                    EmptyValueOptions = fieldTypeAttr.EmptyValue,
                    Dependencies = fieldTypeAttr.Dependencies
                };

                yield return fieldInfo;
            }
        }
    }
}