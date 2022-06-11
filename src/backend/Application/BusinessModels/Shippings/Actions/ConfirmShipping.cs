using Application.BusinessModels.Shared.Actions;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using System;
using System.Linq;

namespace Application.BusinessModels.Shippings.Actions
{
    public class ConfirmShipping : IAppAction<Shipping>
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;

        public AppColor Color { get; set; }

        public ConfirmShipping(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
            Color = AppColor.Green;
        }

        public AppResult Run(CurrentUserDto user, Shipping shipping)
        {
            shipping.Status = ShippingState.ShippingConfirmed;
            shipping.IsNewCarrierRequest = false;

            if (shipping.CarrierId != null)
            {
                var requestsDbSet = _dataService.GetDbSet<CarrierRequestDatesStat>();
                var requestEntry = requestsDbSet.FirstOrDefault(x => x.ShippingId == shipping.Id && x.CarrierId == shipping.CarrierId);
                if (requestEntry == null)
                {
                    requestEntry = new CarrierRequestDatesStat
                    {
                        Id = Guid.NewGuid(),
                        ShippingId = shipping.Id,
                        CarrierId = shipping.CarrierId.Value,
                        SentAt = DateTime.Now
                    };
                    requestsDbSet.Add(requestEntry);
                }
                requestEntry.ConfirmedAt = DateTime.Now;
            }

            var orders = _dataService.GetDbSet<Order>().Where(x => x.ShippingId.HasValue && x.ShippingId.Value == shipping.Id).ToList();
            foreach (Order order in orders)
            {
                order.ShippingStatus = VehicleState.VehicleWaiting;
                order.OrderShippingStatus = shipping.Status;
                order.IsNewCarrierRequest = false;
            }

            _historyService.Save(shipping.Id, "shippingSetConfirmed", shipping.ShippingNumber);

            return new AppResult
            {
                IsError = false,
                Message = "shippingSetConfirmed".Translate(user.Language, shipping.ShippingNumber)
            };
        }

        public bool IsAvailable(Shipping shipping)
        {
            return IsAvailable(shipping.Status);
        }

        public bool IsAvailable(ShippingState? shippingStatus)
        {
            return shippingStatus == ShippingState.ShippingRequestSent;
        }
    }
}
