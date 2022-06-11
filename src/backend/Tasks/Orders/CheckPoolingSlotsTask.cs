using Application.Shared.Triggers;
using Application.Shared.Pooling;
using Application.Shared.Pooling.Models;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Services.Pooling.Models;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Common;

namespace Tasks.Orders
{
    [Description("Проверка признака пулинга")]
    public class CheckPoolingSlotsTask : TaskBase<CheckPoolingSlotsProperties>, IScheduledTask
    {
        public string Schedule { get; set; }

        protected override async Task Execute(IServiceProvider serviceProvider, CheckPoolingSlotsProperties parameters, CancellationToken cancellationToken)
        {
            var dataService = serviceProvider.GetService<ICommonDataService>();
            var userProvider = serviceProvider.GetService<IUserProvider>();
            var historyService = serviceProvider.GetService<IHistoryService>();
            var poolingApiService = serviceProvider.GetService<IPoolingApiService>();
            var triggersService = serviceProvider.GetService<ITriggersService>();

            var user = userProvider.GetCurrentUser();

            var shippings = dataService.GetDbSet<Shipping>()
                  .Include(i => i.Company)
                  .Where(i => i.Status == ShippingState.ShippingSlotBooked)
                  .Where(i => i.ConsolidationDate <= DateTime.Now)
                  .Where(i => !i.SyncedWithPooling)
                  .ToList();

            using (LogContext.PushProperty("Type", "Pooling"))
            {
                foreach (var shipping in shippings)
                {
                    try
                    {
                        Log.Information($"Проверка признака пулинга для перевозки {shipping.ShippingNumber}");

                        var slot = poolingApiService.GetSlot(shipping.SlotId, shipping.Company);
                        var orders = dataService.GetDbSet<Order>().Where(i => i.ShippingId == shipping.Id);

                        // Process error

                        if (!string.IsNullOrEmpty(slot.Error))
                        {
                            Log.Error($"Ошибка получения слота {shipping.SlotId} в { poolingApiService.Url}");
                            LogError(slot, shipping, orders, historyService, user.Language);

                            continue;
                        }

                        // Update shipping

                        var newValue = slot.Result.PalletCount < 25 ? false : slot.Result.ShippingType == "Pooling";
                        var valueChanged = newValue != shipping.IsPooling;

                        if (valueChanged)
                        {
                            shipping.IsPooling = newValue;
                            shipping.SyncedWithPooling = true;

                            orders.Where(i => !string.IsNullOrEmpty(i.BookingNumber)).ToList()
                                .ForEach(i => i.IsPooling = newValue);

                            Log.Information($"Признак пулинга успешно обновлен для перевозки {shipping.ShippingNumber}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Ошибка обновления Признака пулинга для перевозки {shipping?.ShippingNumber}");
                    }
                }
            }

            triggersService.Execute(false);

            dataService.SaveChanges();
        }

        private void LogError(HttpResult<SlotDto> slot, Shipping shipping, IEnumerable<Order> orders, IHistoryService historyService, string lang)
        {
            var messageMap = new Dictionary<HttpStatusCode, string>
            {
                { HttpStatusCode.Unauthorized, "checkPoolingSlotsUnauthorized" },
                { HttpStatusCode.NotFound, "checkPoolingSlotsNotFound" },
                { HttpStatusCode.InternalServerError, "checkPoolingSlotsInternalServerError" }
            };

            if (messageMap.ContainsKey(slot.StatusCode))
            { 
                historyService.Save(shipping.Id, messageMap[slot.StatusCode]);

                orders.ToList().ForEach(i => historyService.Save(i.Id, messageMap[slot.StatusCode]));
            }
        }
    }
}
