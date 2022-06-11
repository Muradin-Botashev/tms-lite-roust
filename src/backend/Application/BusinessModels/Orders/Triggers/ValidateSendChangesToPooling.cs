using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Orders;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Services.Orders;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Application.BusinessModels.Orders.Triggers
{
    public class ValidateSendChangesToPooling : IValidationTrigger<Order>
    {
        private readonly IUserProvider _userProvider;
        private readonly ICommonDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly IOrderPoolingService _poolingService;

        public ValidateSendChangesToPooling(
            IUserProvider userProvider, 
            ICommonDataService dataService, 
            IHistoryService historyService,
            IOrderPoolingService poolingService)
        {
            _userProvider = userProvider;
            _dataService = dataService;
            _historyService = historyService;
            _poolingService = poolingService;
        }

        public ValidateResult Execute(IEnumerable<EntityChanges<Order>> changes)
        {
            var errors = new List<string>();

            foreach (var order in changes.Select(x => x.Entity))
            {
                // брони нет по этому заказу
                if (string.IsNullOrEmpty(order.BookingNumber)) continue;

                var poolingResult = SendChangesToPooling(order, changes.FirstOrDefault(i => i.Entity.Id == order.Id));

                if (poolingResult != null && poolingResult.IsError)
                {
                    errors.Add(poolingResult.Message);
                }
            }

            return new ValidateResult(string.Join("; ", errors));
        }

        public IEnumerable<EntityChanges<Order>> FilterTriggered(IEnumerable<EntityChanges<Order>> changes)
        {
            return changes;
        }

        private ValidateResult SendChangesToPooling(Order order, EntityChanges<Order> change)
        {
            var trackedFields = new[]
            {
                nameof(OrderDto.ShippingWarehouseId),
                nameof(OrderDto.DeliveryWarehouseId),
                nameof(OrderDto.CarrierId),
                nameof(OrderDto.BodyTypeId),
                nameof(OrderDto.VehicleTypeId),
                nameof(OrderDto.ClientOrderNumber),
                nameof(OrderDto.BoxesCount),
                nameof(OrderDto.PalletsCount),
                nameof(OrderDto.ShippingDate),
                nameof(OrderDto.DeliveryDate),
                nameof(OrderDto.DeliveryCost),
                nameof(OrderDto.WeightKg),
                nameof(OrderDto.OrderAmountExcludingVAT)
            };

            var trackedChanges = change.FieldChanges.Where(i => trackedFields.Contains(i.FieldName));

            if (!trackedChanges.Any() || order.ShippingId == null || order.OrderShippingStatus != ShippingState.ShippingSlotBooked)
            {
                return null;
            }

            var user = _userProvider.GetCurrentUser();

            var shipping = _dataService.GetById<Shipping>(order.ShippingId.Value);
            var orders = _dataService.GetDbSet<Order>().Where(x => x.ShippingId == shipping.Id && x.Id != order.Id && !string.IsNullOrEmpty(x.BookingNumber)).ToList();
            orders.Add(order);

            var validationResult = _poolingService.ValidateOrders(orders, user);

            if (validationResult != null && validationResult.IsError)
            {
                return new ValidateResult(validationResult.Message, order.Id, true);
            }

            var fieldNames = trackedChanges.Select(i => i.FieldName.ToLowerFirstLetter().Translate(user.Language));

            Log.Information($"Обновление брони пулинга заказа {order.OrderNumber} по полям: {string.Join(", ", fieldNames)}");

            var result = _poolingService.UpdateSlot(shipping, orders);

            if (result.IsError)
            {
                var errorsMap = new Dictionary<HttpStatusCode, string>
                {
                    { HttpStatusCode.Unauthorized, "poolingChangeUnauthorized" },
                    { HttpStatusCode.Forbidden, "poolingChangeForbidden" },
                    { HttpStatusCode.NotFound, "poolingChangeNotFound" },
                    { HttpStatusCode.InternalServerError, "poolingChangeInternalServerError" },
                };

                var shippingErrorsMap = new Dictionary<HttpStatusCode, string>
                {
                    { HttpStatusCode.Unauthorized, "poolingChangeShippingUnauthorized" },
                    { HttpStatusCode.Forbidden, "poolingChangeShippingForbidden" },
                    { HttpStatusCode.NotFound, "poolingChangeShippingNotFound" },
                    { HttpStatusCode.InternalServerError, "poolingChangeShippingInternalServerError" },
                };

                if (errorsMap.ContainsKey(result.StatusCode))
                {
                    _historyService.Save(order.Id, errorsMap[result.StatusCode], string.Join(", ", fieldNames));
                    _historyService.Save(shipping.Id, shippingErrorsMap[result.StatusCode], string.Join(", ", fieldNames), order.OrderNumber);
                }

                return new ValidateResult(result.Error, order.Id, true);
            }
            else
            {
                var isPooling = result.Result.ShippingType == "Pooling";

                shipping.IsPooling = isPooling;
                orders.ToList().ForEach(x => x.IsPooling = isPooling);
            }

            foreach (var fieldChange in trackedChanges)
            {
                var fieldName = fieldChange.FieldName.ToLowerFirstLetter().Translate(user.Language);
                _historyService.Save(order.Id, "poolingOrderChangeHistory", fieldName);
                _historyService.Save(shipping.Id, "poolingShippingChangeHistory", fieldName, order.OrderNumber);
            }

            return new ValidateResult
            {
                IsError = false,
                Message = "poolingOrderChangeFields".Translate(user.Language, string.Join(", ", fieldNames)),
                Id = order.Id
            };
        }
    }
}
