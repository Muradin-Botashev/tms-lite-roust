using Application.BusinessModels.Shared.Actions;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Orders.Actions
{
    /// <summary>
    /// В архив
    /// </summary>
    [ActionGroup(nameof(Order)), OrderNumber(8), DescriptionKey("sendToArchiveDescription")]
    public class SendToArchive : IAppAction<Order>
    {
        private readonly IHistoryService _historyService;

        private readonly ICommonDataService _dataService;

        public SendToArchive(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
            Color = AppColor.Blue;
        }

        public AppColor Color { get; set; }

        public AppResult Run(CurrentUserDto user, Order order)
        {
            order.Status = OrderState.Archive;

            _historyService.Save(order.Id, "orderSetArchived", order.OrderNumber);
            
            return new AppResult
            {
                IsError = false,
                Message = "orderSetArchived".Translate(user.Language, order.OrderNumber)
            };
        }

        public bool IsAvailable(Order order)
        {
            return (order.Status == OrderState.Delivered && (!order.DeliveryType.HasValue || order.DeliveryType.Value == DeliveryType.Delivery)) ||
                   (order.Status == OrderState.Delivered && (!order.DeliveryType.HasValue || order.DeliveryType.Value == DeliveryType.Courier)) ||
                   (order.Status == OrderState.Shipped && (order.DeliveryType.HasValue && order.DeliveryType.Value == DeliveryType.SelfDelivery));
        }
    }
}