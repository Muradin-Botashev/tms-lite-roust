using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.FieldProperties;
using Domain.Services.Orders;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FieldProperties
{
    public class FieldPropertiesService : IFieldPropertiesService
    {
        private readonly ICommonDataService _dataService;
        private readonly IFieldDispatcherService _fieldDispatcherService;
        private readonly IUserProvider _userProvider;
        private static readonly string ShowIdentifier = FieldPropertiesAccessType.Show.FormatEnum();

        public FieldPropertiesService(ICommonDataService dataService, IFieldDispatcherService fieldDispatcherService, IUserProvider userProvider)
        {
            _dataService = dataService;
            _fieldDispatcherService = fieldDispatcherService;
            _userProvider = userProvider;
        }

        public List<FieldPropertyItem> LoadMatrixItems(Guid? companyId, Guid? roleId, Guid? userId)
        {
            if (roleId == null && userId != null)
            {
                roleId = _dataService.GetById<User>(userId.Value)?.RoleId;
            }

            var result = _dataService.GetDbSet<FieldPropertyItem>()
                                     .Where(x => (x.RoleId == roleId || x.RoleId == null)
                                              && (x.CompanyId == companyId || x.CompanyId == null))
                                     .ToList();
            return result;
        }

        public List<FieldPropertyItemVisibility> LoadVisibilities(Guid? companyId, Guid? roleId, Guid? userId)
        {
            if (roleId == null && userId != null)
            {
                roleId = _dataService.GetById<User>(userId.Value)?.RoleId;
            }

            var result = _dataService.GetDbSet<FieldPropertyItemVisibility>()
                                     .Where(x => (x.RoleId == roleId || x.RoleId == null)
                                              && (x.CompanyId == companyId || x.CompanyId == null))
                                     .ToList();
            return result;
        }

        public IEnumerable<FieldForFieldProperties> GetFor(string forEntity, Guid? companyId, Guid? roleId, Guid? userId,
                                                           List<FieldPropertyItem> matrixItems = null,
                                                           List<FieldPropertyItemVisibility> visibilities = null)
        {
            var result = new List<FieldForFieldProperties>();

            var forEntityType = forEntity.ToEnum<FieldPropertiesForEntityType>() ?? default;

            Array states = forEntityType == FieldPropertiesForEntityType.Shippings || forEntityType == FieldPropertiesForEntityType.RoutePoints
                ? Enum.GetValues(typeof(ShippingState))
                : Enum.GetValues(typeof(OrderState));

            var user = userId == null ? null : _dataService.GetById<User>(userId.Value);

            if (companyId == null && user != null)
                companyId = user.CompanyId;

            if (roleId == null && user != null)
                roleId = user.RoleId;

            string lang = _userProvider.GetCurrentUser()?.Language;

            var fieldMatrixItems = (matrixItems == null ? _dataService.GetDbSet<FieldPropertyItem>().AsQueryable() : matrixItems.AsQueryable())
                                               .Where(x => x.ForEntity == forEntityType
                                                        && (x.RoleId == roleId || x.RoleId == null)
                                                        && (x.CompanyId == companyId || x.CompanyId == null))
                                               .ToList();

            var fieldVisibilities = (visibilities == null ? _dataService.GetDbSet<FieldPropertyItemVisibility>().AsQueryable() : visibilities.AsQueryable())
                                               .Where(x => x.ForEntity == forEntityType
                                                        && (x.RoleId == roleId || x.RoleId == null)
                                                        && (x.CompanyId == companyId || x.CompanyId == null))
                                               .ToList();

            var fieldNames = GetFieldNames(forEntityType);
            foreach (var fieldName in fieldNames)
            {
                var accessTypes = new Dictionary<string, string>();

                var visibilitySetting = fieldVisibilities.SingleOrDefault(x => x.FieldName == fieldName.Name);

                var isHidden = visibilitySetting?.IsHidden ?? false;
                
                foreach (var state in states)
                {
                    var stateName = state.FormatEnum();
                    if (isHidden)
                        accessTypes[stateName] = ShowIdentifier;
                    else
                    {
                        var stateId = (int)state;

                        var fieldMatrixItem =
                            fieldMatrixItems.Where(x => x.State == stateId && x.FieldName == fieldName.Name)
                                .OrderBy(x => x)
                                .FirstOrDefault();

                        var accessType = fieldMatrixItem?.AccessType.FormatEnum() ?? ShowIdentifier;

                        if (!string.IsNullOrEmpty(stateName))
                        {
                            accessTypes[stateName] = fieldName.IsReadOnly ? ShowIdentifier : accessType;
                        }                    
                    }
                }
                result.Add(new FieldForFieldProperties
                {
                    FieldName = fieldName.Name,
                    DisplayName = fieldName.DisplayNameKey.Translate(lang),
                    AccessTypes = accessTypes,
                    isReadOnly = fieldName.IsReadOnly,
                    isHidden = isHidden
                });
            }

            return result;
        }

        public string GetAccessTypeForField(GetForFieldPropertyParams dto)
        {
            var currentUser = _userProvider.GetCurrentUser();

            var forEntity = dto.ForEntity.ToEnum<FieldPropertiesForEntityType>() ?? default;

            int state = forEntity == FieldPropertiesForEntityType.Shippings || forEntity == FieldPropertiesForEntityType.RoutePoints
                ? (int)(dto.State.ToEnum<ShippingState>() ?? default)
                : (int)(dto.State.ToEnum<OrderState>() ?? default);

            var fieldNames = GetFieldNames(forEntity);
            var field = fieldNames.FirstOrDefault(i => i.Name.ToLower() == dto.FieldName.ToLower());

            var fieldMatrixItem = _dataService.GetDbSet<FieldPropertyItem>()
                                              .Where(x => x.ForEntity == forEntity
                                                        && x.FieldName == dto.FieldName
                                                        && x.State == state
                                                        && (x.CompanyId == currentUser.CompanyId || x.CompanyId == null)
                                                        && (x.RoleId == currentUser.RoleId || x.RoleId == null))
                                              .OrderBy(x => x)
                                              .FirstOrDefault();

            var accessType = !field.IsReadOnly && fieldMatrixItem != null ? fieldMatrixItem.AccessType : FieldPropertiesAccessType.Show;

            return accessType.FormatEnum();
        }

        public IEnumerable<string> GetAvailableFields(
            FieldPropertiesForEntityType forEntityType, 
            Guid? companyId, 
            Guid? roleId, 
            Guid? userId,
            List<FieldPropertyItem> matrixItems = null,
            List<FieldPropertyItemVisibility> visibilities = null)
        {
            var result = new List<string>();
            var hiddenAccessType = FieldPropertiesAccessType.Hidden.FormatEnum();
            var fieldProperties = GetFor(forEntityType.ToString(), companyId, roleId, userId, matrixItems, visibilities)
                .Where(x=> !x.isHidden);
            foreach (var prop in fieldProperties)
            {
                bool hasAccess = prop.AccessTypes.Any(x => x.Value != hiddenAccessType);
                if (hasAccess)
                {
                    result.Add(prop.FieldName);
                }
            }
            return result;
        }

        public IEnumerable<string> GetReadOnlyFields(
            FieldPropertiesForEntityType forEntityType, 
            string stateName, 
            Guid? companyId, 
            Guid? roleId, 
            Guid? userId,
            List<FieldPropertyItem> matrixItems = null,
            List<FieldPropertyItemVisibility> visibilities = null)
        {
            var result = new List<string>();
            var editAccessType = FieldPropertiesAccessType.Edit.FormatEnum();
            var fieldProperties = GetFor(forEntityType.ToString(), companyId, roleId, userId, matrixItems, visibilities);
            foreach (var prop in fieldProperties)
            {
                bool isReadOnly = true;
                if (prop.AccessTypes.TryGetValue(stateName, out string accessType))
                {
                    isReadOnly = accessType != editAccessType;
                }
                if (isReadOnly)
                {
                    result.Add(prop.FieldName);
                }
            }
            return result;
        }

        public FieldPropertiesAccessType GetFieldAccess(
            FieldPropertiesForEntityType forEntityType,
            int state, 
            string fieldName,
            Guid? companyId, 
            Guid? roleId, 
            Guid? userId)
        {
            var fieldMatrixItem = _dataService.GetDbSet<FieldPropertyItem>()
                                              .Where(x => x.ForEntity == forEntityType
                                                        && x.FieldName == fieldName
                                                        && x.State == state
                                                        && (x.RoleId == roleId || x.RoleId == null)
                                                        && (x.CompanyId == companyId || x.CompanyId == null))
                                              .OrderBy(x => x)
                                              .FirstOrDefault();
            return fieldMatrixItem?.AccessType ?? FieldPropertiesAccessType.Show;
        }

        public ValidateResult ToggleHiddenState(ToggleHiddenStateDto dto)
        {
            var dbSet = _dataService.GetDbSet<FieldPropertyItemVisibility>();

            var forEntity = dto.ForEntity.ToEnum<FieldPropertiesForEntityType>() ?? default;

            var companyId = dto.CompanyId.ToGuid();
            var roleId = dto.RoleId.ToGuid();
            
            var visibilityItem = dbSet.SingleOrDefault(x => x.ForEntity == forEntity
                                                            && x.CompanyId == companyId
                                                            && x.RoleId == roleId
                                                            && x.FieldName == dto.FieldName);

            if (visibilityItem == null)
            {
                visibilityItem = new FieldPropertyItemVisibility
                {
                    Id = Guid.NewGuid(),
                    ForEntity = forEntity,
                    CompanyId = companyId,
                    RoleId = roleId,
                    FieldName = dto.FieldName,
                    IsHidden = true
                };
                dbSet.Add(visibilityItem);
            }
            else
                visibilityItem.IsHidden = !visibilityItem.IsHidden;

            _dataService.SaveChanges();

            return new ValidateResult();
        }

        public ValidateResult Save(FieldPropertyDto dto)
        {
            var dbSet = _dataService.GetDbSet<FieldPropertyItem>();

            var forEntity = dto.ForEntity.ToEnum<FieldPropertiesForEntityType>() ?? default;

            var companyId = dto.CompanyId.ToGuid();
            var roleId = dto.RoleId.ToGuid();

            var entities = dbSet.Where(x => x.ForEntity == forEntity
                                        && x.RoleId == roleId
                                        && x.CompanyId == companyId
                                        && x.FieldName == dto.FieldName)
                                .ToList();

            var states = GetStates(forEntity, dto.State);

            foreach (var state in states)
            {
                var stateId = (int)state;
                var entity = entities.Where(x => x.State == stateId).FirstOrDefault();

                if (entity == null)
                {
                    entity = new FieldPropertyItem
                    {
                        Id = Guid.NewGuid(),
                        ForEntity = forEntity,
                        CompanyId = companyId,
                        RoleId = roleId,
                        FieldName = dto.FieldName,
                        State = stateId
                    };
                    dbSet.Add(entity);
                }

                entity.AccessType = dto.AccessType.ToEnum<FieldPropertiesAccessType>() ?? default;
            }

            _dataService.SaveChanges();

            return new ValidateResult();
        }

        public IEnumerable<LookUpDto> GetCompanies()
        {
            var query = _dataService.GetDbSet<Company>().AsQueryable();

            var currentUser = _userProvider.GetCurrentUser();
            if (currentUser?.CompanyId != null)
            {
                query = query.Where(x => x.Id == currentUser.CompanyId.Value);
            }
            else
            {
                yield return new LookUpDto
                {
                    Name = "global".Translate(currentUser?.Language),
                    Value = LookUpDto.EmptyValue
                };
            }

            var entities = query.Where(i => i.IsActive)
                                .OrderBy(x => x.Name)
                                .ToList();

            foreach (var entity in entities)
            {
                yield return new LookUpDto
                {
                    Name = entity.Name,
                    Value = entity.Id.FormatGuid(),
                };
            }
        }

        private List<FieldInfo> GetFieldNames(FieldPropertiesForEntityType entityType)
        {
            switch(entityType)
            {
                case FieldPropertiesForEntityType.Orders:
                    return ExtractFieldNamesFromDto<OrderDto>();
                case FieldPropertiesForEntityType.OrderItems:
                    return ExtractFieldNamesFromDto<OrderItemDto>();
                case FieldPropertiesForEntityType.RoutePoints:
                    return ExtractFieldNamesFromDto<RoutePointDto>();
                case FieldPropertiesForEntityType.Shippings:
                    return ExtractFieldNamesFromDto<ShippingDto>();
                default:
                    return new List<FieldInfo>();
            }
        }

        private static Array GetStates(FieldPropertiesForEntityType entityType, string state = "")
        {
            Array states;
            if (string.IsNullOrEmpty(state))
            {
                states = entityType == FieldPropertiesForEntityType.Shippings ||
                         entityType == FieldPropertiesForEntityType.RoutePoints
                    ? Enum.GetValues(typeof(ShippingState))
                    : Enum.GetValues(typeof(OrderState));
            }
            else
            {
                states = new[]
                {
                    entityType == FieldPropertiesForEntityType.Shippings ||
                    entityType == FieldPropertiesForEntityType.RoutePoints
                        ? (int)(state.ToEnum<ShippingState>() ?? default)
                        : (int)(state.ToEnum<OrderState>() ?? default)
                };
            }

            return states;
        }

        private List<FieldInfo> ExtractFieldNamesFromDto<TDto>()
        {
            var result = _fieldDispatcherService.GetDtoFields<TDto>()
                .ToList();
            
            foreach (var fieldInfo in result) 
                fieldInfo.Name = fieldInfo.Name?.ToLowerFirstLetter();

            return result;
        }
    }
}