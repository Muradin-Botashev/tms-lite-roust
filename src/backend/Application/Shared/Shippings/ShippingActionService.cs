using Application.Shared.Notifications;
using Application.Shared.Orders;
using Application.Shared.TransportCompanies;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.Shippings
{
    public class ShippingActionService : IShippingActionService
    {
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly IShippingTarifficationTypeDeterminer _shippingTarifficationTypeDeterminer;
        private readonly IShippingCalculationService _shippingCalculationService;
        private readonly IDeliveryCostCalcService _deliveryCostCalcService;
        private readonly IDriverDataSyncService _driverDataSyncService;
        private readonly IOrderPoolingService _poolingService;
        private readonly INotificationService _notificationService;
        private readonly ICarrierSelectionService _carrierSelectionService;
        private readonly IUserProvider _userProvider;

        public ShippingActionService(
            ICommonDataService dataService, 
            IHistoryService historyService,
            IShippingTarifficationTypeDeterminer shippingTarifficationTypeDeterminer,
            IShippingCalculationService shippingCalculationService,
            IDeliveryCostCalcService deliveryCostCalcService,
            IDriverDataSyncService driverDataSyncService,
            IOrderPoolingService poolingService,
            INotificationService notificationService,
            ICarrierSelectionService carrierSelectionService,
            IUserProvider userProvider)
        {
            _dataService = dataService;
            _historyService = historyService;
            _shippingCalculationService = shippingCalculationService;
            _shippingTarifficationTypeDeterminer = shippingTarifficationTypeDeterminer;
            _deliveryCostCalcService = deliveryCostCalcService;
            _driverDataSyncService = driverDataSyncService;
            _poolingService = poolingService;
            _notificationService = notificationService;
            _carrierSelectionService = carrierSelectionService;
            _userProvider = userProvider;
        }

        public AppResult BaseRejectShippingRequest(Shipping shipping, List<Order> orders = null)
        {
            if (orders == null)
            {
                orders = _dataService.GetDbSet<Order>().Where(i => i.ShippingId == shipping.Id).ToList();
            }

            shipping.Status = ShippingState.ShippingRejectedByTc;
            shipping.IsNewCarrierRequest = false;

            if (shipping.TarifficationType != TarifficationType.Milkrun && shipping.TarifficationType != TarifficationType.Pooling)
            {
                _notificationService.SendRejectShippingRequestNotification(shipping.Id);
            }

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
                requestEntry.RejectedAt = DateTime.Now;
            }

            foreach (var order in orders)
            {
                order.OrderShippingStatus = shipping.Status;
                order.IsNewCarrierRequest = false;
            }

            _historyService.Save(shipping.Id, "shippingSetRejected", shipping.ShippingNumber);

            var lang = _userProvider.GetCurrentUser()?.Language;
            return new AppResult
            {
                IsError = false,
                Message = "shippingSetRejected".Translate(lang, shipping.ShippingNumber)
            };
        }

        public AppResult RejectShippingRequest(Shipping shipping, List<Order> orders = null)
        {
            var result = BaseRejectShippingRequest(shipping, orders);

            var rejectAction = new CarrierShippingAction
            {
                Id = Guid.NewGuid(),
                CarrierId = shipping.CarrierId,
                ShippingId = shipping.Id,
                ActionName = "Отклонение перевозки",
                ActionTime = DateTime.Now
            };
            _dataService.GetDbSet<CarrierShippingAction>().Add(rejectAction);

            _carrierSelectionService.FindAndUpdateCarrier(shipping, orders, shipping.CarrierId);

            return result;
        }

        public Shipping UnionOrders(IEnumerable<Order> orders, string shippingNumber = null, bool forRegroupping = false)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            var shippingDbSet = _dataService.GetDbSet<Shipping>();

            var companyId = orders.Select(x => x.CompanyId).FirstOrDefault();
            var company = companyId == null ? null : _dataService.GetById<Company>(companyId.Value);

            var shipping = new Shipping
            {
                Status = ShippingState.ShippingCreated,
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ShippingNumber = shippingNumber ?? ShippingNumberProvider.GetNextShippingNumber(),
                ShippingCreationDate = DateTime.UtcNow,
                PoolingProductType = company?.PoolingProductType,
                BottlesCount = orders.Sum(x => x.BottlesCount),
                Volume9l = orders.Sum(x => x.Volume9l)
            };

            shipping.DeliveryType = DeliveryType.Delivery;
            shipping.TarifficationType = _shippingTarifficationTypeDeterminer.GetTarifficationTypeForOrders(shipping, orders);

            var vehicleTypeIds = orders.Select(x => x.VehicleTypeId)
                                       .Where(x => x.HasValue)
                                       .Distinct();
            shipping.VehicleTypeId = vehicleTypeIds.Count() > 1 ? null : vehicleTypeIds.FirstOrDefault();

            var bodyTypeIds = orders.Select(x => x.BodyTypeId)
                                    .Where(x => x.HasValue)
                                    .Distinct();

            if (bodyTypeIds.Count() > 1)
            {
                throw new DomainException("multiBodyTypeError".Translate(lang));
            }
            else
            {
                shipping.BodyTypeId = bodyTypeIds.FirstOrDefault();
            }

            shippingDbSet.Add(shipping);

            var historyMessage = forRegroupping ? "shippingSetCreatedRegroupping" : "shippingSetCreated";
            _historyService.Save(shipping.Id, historyMessage, shipping.ShippingNumber);

            UnionOrderInShipping(orders, orders, shipping);

            if (!forRegroupping)
            {
                orders.ToList().ForEach(i =>
                {
                    _historyService.Save(i.Id, "orderSetInShipping", i.OrderNumber, shipping.ShippingNumber);
                    _historyService.Save(shipping.Id, "shippingAddOrder", i.OrderNumber, shipping.ShippingNumber);
                });
            }

            FillCarrierId(orders, shipping);

            _deliveryCostCalcService.UpdateDeliveryCost(shipping, orders);
            _shippingCalculationService.RecalculateDeliveryCosts(shipping, orders);

            _driverDataSyncService.SyncDriverProperties(shipping, orders);

            _shippingCalculationService.RecalculateShippingOrdersCosts(shipping, orders);

            return shipping;
        }

        public AppResult UnionOrdersInExisted(IEnumerable<Order> orders)
        {
            var shippingId = orders.Single(x => x.Status == OrderState.InShipping).ShippingId;
            var shipping = _dataService.GetById<Shipping>(shippingId.Value);

            orders = orders.Where(x => x.Status != OrderState.InShipping).ToList();

            return UnionOrdersInExisted(shipping, orders);
        }

        public AppResult UnionOrdersInExisted(Shipping shipping, IEnumerable<Order> orders)
        {
            if (shipping.Status == ShippingState.ShippingConfirmed)
            {
                shipping.Status = ShippingState.ShippingRequestSent;

                string orderNumbers = string.Join(", ", orders.Select(x => x.OrderNumber));
                _historyService.Save(shipping.Id, "shippingAddOrdersResendRequest", shipping.ShippingNumber, orderNumbers);
            }

            var allOrders = _dataService.GetDbSet<Order>().Where(x => x.ShippingId == shipping.Id).ToList();
            allOrders.AddRange(orders);

            allOrders.ToList().ForEach(i => i.BodyTypeId = shipping.BodyTypeId);

            UnionOrderInShipping(allOrders, orders, shipping);

            orders.ToList().ForEach(i =>
            {
                _historyService.Save(i.Id, "orderSetInShipping", i.OrderNumber, shipping.ShippingNumber);
                _historyService.Save(shipping.Id, "shippingAddOrder", i.OrderNumber, shipping.ShippingNumber);
            });

            FillCarrierId(allOrders, orders, shipping);

            _deliveryCostCalcService.UpdateDeliveryCost(shipping, allOrders, true);
            _shippingCalculationService.RecalculateDeliveryCosts(shipping, allOrders);
            _shippingCalculationService.RecalculateShippingOrdersCosts(shipping, allOrders);

            if (shipping.Status == ShippingState.ShippingSlotBooked)
            {
                var updateSlotResult = _poolingService.UpdateSlot(shipping, allOrders);
                if (updateSlotResult.IsError)
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = updateSlotResult.Error
                    };
                }
                else
                {
                    var slot = _poolingService.GetSlot(shipping.SlotId, shipping.CompanyId);
                    var isPooling = slot?.Result?.ShippingType == "Pooling";

                    shipping.IsPooling = isPooling;
                    allOrders.ToList().ForEach(x => x.IsPooling = isPooling);
                }
            }

            if (shipping.Status == ShippingState.ShippingRequestSent
                && shipping.TarifficationType != TarifficationType.Milkrun 
                && shipping.TarifficationType != TarifficationType.Pooling)
            {
                var notificationData = new NotificationOrdersListDto
                {
                    Orders = orders.Select(x => new NotificationOrderDto
                    {
                        OrderNumber = x.OrderNumber,
                        DeliveryAddress = x.DeliveryAddress
                    }).ToList()
                };
                _notificationService.SendAddOrdersToShippingNotification(shipping.Id, notificationData);
            }

            var lang = _userProvider.GetCurrentUser()?.Language;
            return new AppResult
            {
                IsError = false,
                Message = "shippingSetCreated".Translate(lang, shipping.ShippingNumber)
            };
        }

        private void UnionOrderInShipping(IEnumerable<Order> allOrders, IEnumerable<Order> newOrders, Shipping shipping)
        {
            _shippingCalculationService.RecalculateShipping(shipping, allOrders);

            foreach (var order in allOrders)
            {
                order.ShippingNumber = shipping.ShippingNumber;
                order.OrderShippingStatus = shipping.Status;
            }

            var bookingNumber = allOrders.Select(x => x.BookingNumber).FirstOrDefault(x => !string.IsNullOrEmpty(x));

            foreach (var order in newOrders)
            {
                order.ShippingId = shipping.Id;
                order.Status = OrderState.InShipping;

                order.BookingNumber = bookingNumber;

                order.IsNewForConfirmed = false;
                order.IsNewCarrierRequest = shipping.IsNewCarrierRequest;

                order.VehicleTypeId = shipping.VehicleTypeId;
                order.BodyTypeId = shipping.BodyTypeId;
                order.TarifficationType = shipping.TarifficationType;

                order.ShippingStatus = VehicleState.VehicleWaiting;
                order.DeliveryStatus = VehicleState.VehicleEmpty;
            }
        }

        private void FillCarrierId(IEnumerable<Order> orders, Shipping shipping)
        {
            var carrierIds = orders.Where(x => x.CarrierId != null)
                                   .Select(x => x.CarrierId)
                                   .Distinct();

            if (carrierIds.Count() > 1 || !carrierIds.Any())
            {
                shipping.CarrierId = null;
            }
            else
            {
                shipping.CarrierId = carrierIds.First();
            }

            foreach (var order in orders)
            {
                order.CarrierId = shipping.CarrierId;
            }
        }

        private void FillCarrierId(IEnumerable<Order> allOrders, IEnumerable<Order> orders, Shipping shipping)
        {
            var newCarrierIds = orders.Where(x => x.CarrierId != null)
                                      .Select(x => x.CarrierId)
                                      .Distinct();

            var newCarrierId = newCarrierIds.FirstOrDefault();
            if (newCarrierIds.Count() > 1)
            {
                newCarrierId = null;
            }

            if (shipping.CarrierId == null && newCarrierId != null)
            {
                shipping.CarrierId = newCarrierId;
                foreach (var order in allOrders)
                {
                    order.CarrierId = shipping.CarrierId;
                }
            }
            else
            {
                foreach (var order in orders)
                {
                    order.CarrierId = shipping.CarrierId;
                }
            }
        }
    }
}
