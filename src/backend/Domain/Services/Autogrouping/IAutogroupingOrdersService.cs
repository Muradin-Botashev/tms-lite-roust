using Domain.Services.AppConfiguration;
using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public interface IAutogroupingOrdersService
    {
        SearchResult<AutogroupingOrderDto> Search(Guid runId, Guid? parentId, FilterFormDto<AutogroupingOrdersFilterDto> dto);
        IEnumerable<LookUpDto> ForSelect(Guid runId, Guid? parentId, string field, FilterFormDto<AutogroupingOrdersFilterDto> form);
        UserConfigurationGridItem GetPreviewConfiguration();
    }
}
