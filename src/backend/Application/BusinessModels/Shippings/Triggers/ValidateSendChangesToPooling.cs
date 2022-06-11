using Application.BusinessModels.Shared.Triggers;
using Application.Shared.Orders;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Application.BusinessModels.Shippings.Triggers
{
    public class ValidateSendChangesToPooling : IValidationTrigger<Shipping>
    {
        private readonly IUserProvider _userProvider;
        private readonly IHistoryService _historyService;
        private readonly IOrderPoolingService _poolingService;

        public ValidateSendChangesToPooling(
            IUserProvider userProvider, 
            IHistoryService historyService,
            IOrderPoolingService poolingService)
        {
            _userProvider = userProvider;
            _historyService = historyService;
            _poolingService = poolingService;
        }

        public ValidateResult Execute(IEnumerable<EntityChanges<Shipping>> changes)
        {
            var errors = new List<string>();

            foreach (var shipping in changes.Select(x => x.Entity))
            {
                var poolingResult = SendChangesToPooling(shipping, changes.FirstOrDefault(i => i.Entity.Id == shipping.Id));
                if (poolingResult != null && poolingResult.IsError)
                {
                    errors.Add(poolingResult.Message);
                }
            }

            return new ValidateResult(string.Join("; ", errors));
        }

        public IEnumerable<EntityChanges<Shipping>> FilterTriggered(IEnumerable<EntityChanges<Shipping>> changes)
        {
            return changes;
        }

        private ValidateResult SendChangesToPooling(Shipping shipping, EntityChanges<Shipping> change)
        {
            var trackedFields = new[]
            {
                nameof(ShippingDto.CarrierId),
                nameof(ShippingDto.BodyTypeId),
                nameof(ShippingDto.VehicleTypeId),
            };

            var trackedChanges = change.FieldChanges.Where(i => trackedFields.Contains(i.FieldName));

            if (!trackedChanges.Any() || shipping.Status != ShippingState.ShippingSlotBooked)
            {
                return null;
            }

            var user = _userProvider.GetCurrentUser();

            var fieldNames = trackedChanges.Select(i => i.FieldName.ToLowerFirstLetter().Translate(user.Language));

            Log.Information($"Обновление брони пулинга перевозки {shipping.ShippingNumber} по полям: {string.Join(", ", fieldNames)}");

            var result = _poolingService.UpdateSlot(shipping);

            if (result.IsError)
            {
                var errorsMap = new Dictionary<HttpStatusCode, string>
                {
                    { HttpStatusCode.Unauthorized, "poolingChangeUnauthorized" },
                    { HttpStatusCode.Forbidden, "poolingChangeForbidden" },
                    { HttpStatusCode.NotFound, "poolingChangeNotFound" },
                    { HttpStatusCode.InternalServerError, "poolingChangeInternalServerError" },
                };

                if (errorsMap.ContainsKey(result.StatusCode))
                {
                    var errorMessage = errorsMap[result.StatusCode].Translate(user.Language, string.Join(", ", fieldNames));
                    _historyService.Save(shipping.Id, errorMessage);
                    Log.Error($"Ошибка обновления брони пулинга перевозки {shipping.ShippingNumber} по полям: { errorMessage }");
                }
                else
                {
                    Log.Error($"Ошибка обновления брони пулинга перевозки {shipping.ShippingNumber} по полям: { result.Error }");
                }

                return new ValidateResult(result.Error, shipping.Id, true);
            }

            foreach (var fieldChange in trackedChanges)
            {
                var fieldName = fieldChange.FieldName.ToLowerFirstLetter().Translate(user.Language);
                _historyService.Save(shipping.Id, "poolingOrderChangeHistory".Translate(user.Language, fieldName));
            }

            return new ValidateResult
            {
                IsError = false,
                Message = "poolingOrderChangeFields".Translate(user.Language, string.Join(", ", fieldNames)),
                Id = shipping.Id
            };
        }
    }
}
