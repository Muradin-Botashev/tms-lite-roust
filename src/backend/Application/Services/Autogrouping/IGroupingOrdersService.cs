using Domain.Enums;
using Domain.Persistables;
using Domain.Services.Autogrouping;
using Domain.Shared;
using System;
using System.Collections.Generic;

namespace Application.Services.Autogrouping
{
    public interface IGroupingOrdersService
    {
        AutogroupingResultData GroupOrders(IEnumerable<IAutogroupingOrder> orders, Guid runId, List<AutogroupingType> types);
        AutogroupingResultData GroupOrders(IEnumerable<IAutogroupingOrder> orders, Guid runId, List<AutogroupingType> types, out Dictionary<IAutogroupingOrder, string> skippedOrders);
        ValidateResult MoveOrders(IEnumerable<AutogroupingOrder> orders, AutogroupingShipping targetShipping, List<AutogroupingType> types);
    }
}