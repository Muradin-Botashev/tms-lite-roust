using Domain.Services.AppConfiguration;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.IO;

namespace Domain.Services.Autogrouping
{
    public interface IAutogroupingService
    {
        RunResponse RunGrouping(RunRequest request);
        void ChangeCarrier(Guid runId, ChangeCarrierRequest request);
        ValidateResult MoveOrders(Guid runId, MoveOrderRequest request);
        OperationDetailedResult Apply(Guid runId, List<Guid> rowIds);
        OperationDetailedResult ApplyAndSend(Guid runId, List<Guid> rowIds);
        SearchResult<AutogroupingShippingDto> Search(Guid runId, FilterFormDto<AutogroupingFilterDto> dto);
        IEnumerable<string> SearchIds(Guid runId, FilterFormDto<AutogroupingFilterDto> dto);
        IEnumerable<LookUpDto> ForSelect(Guid runId, string field, FilterFormDto<AutogroupingFilterDto> form);
        AutogroupingSummaryDto GetSummary(Guid runId);
        Stream ExportToExcel(Guid runId, ExportExcelFormDto<AutogroupingFilterDto> dto);
        UserConfigurationGridItem GetPreviewConfiguration();
        AutogroupingTypesDto GetAutogroupingTypes();
    }
}
