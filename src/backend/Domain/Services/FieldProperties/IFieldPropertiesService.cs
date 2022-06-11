using Domain.Enums;
using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.FieldProperties
{
    public interface IFieldPropertiesService
    {
        IEnumerable<FieldForFieldProperties> GetFor(string forEntity, Guid? companyId, Guid? roleId, Guid? userId,
                                                    List<FieldPropertyItem> matrixItems = null,
                                                    List<FieldPropertyItemVisibility> visibilities = null);

        string GetAccessTypeForField(GetForFieldPropertyParams args);
        ValidateResult Save(FieldPropertyDto fieldPropertiesDto);

        List<FieldPropertyItem> LoadMatrixItems(Guid? companyId, Guid? roleId, Guid? userId);
        List<FieldPropertyItemVisibility> LoadVisibilities(Guid? companyId, Guid? roleId, Guid? userId);

        IEnumerable<string> GetAvailableFields(FieldPropertiesForEntityType forEntityType, Guid? companyId, Guid? roleId, Guid? userId,
                                               List<FieldPropertyItem> matrixItems = null,
                                               List<FieldPropertyItemVisibility> visibilities = null);

        IEnumerable<string> GetReadOnlyFields(FieldPropertiesForEntityType forEntityType, string stateName, Guid? companyId, Guid? roleId, Guid? userId,
                                              List<FieldPropertyItem> matrixItems = null,
                                              List<FieldPropertyItemVisibility> visibilities = null);

        FieldPropertiesAccessType GetFieldAccess(FieldPropertiesForEntityType forEntityType, int state, string fieldName, Guid? companyId, Guid? roleId, Guid? userId);
        ValidateResult ToggleHiddenState(ToggleHiddenStateDto dto);

        IEnumerable<LookUpDto> GetCompanies();
    }
}