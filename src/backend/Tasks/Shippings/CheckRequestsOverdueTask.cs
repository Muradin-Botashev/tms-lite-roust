using Application.Shared.Shippings;
using Application.Shared.TransportCompanies;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Common;

namespace Tasks.Shippings
{
    [Description("Поиск просроченных заявок в ТК")]
    public class CheckRequestsOverdueTask : TaskBase<PropertiesBase>, IScheduledTask
    {
        public string Schedule => "*/5 * * * *";

        protected override async Task Execute(IServiceProvider serviceProvider, PropertiesBase parameters, CancellationToken cancellationToken)
        {
            var dataService = serviceProvider.GetService<ICommonDataService>();
            var triggersService = serviceProvider.GetService<ITriggersService>();
            var shippingActionService = serviceProvider.GetService<IShippingActionService>();
            var carrierSelectionService = serviceProvider.GetService<ICarrierSelectionService>();

            var actionsDbSet = dataService.GetDbSet<CarrierShippingAction>();

            try
            {
                var allRequests = await dataService.GetDbSet<CarrierRequestDatesStat>()
                                                   .Include(x => x.Carrier)
                                                   .Include(x => x.Shipping)
                                                   .Where(x => x.Carrier != null
                                                            && x.Carrier.RequestReviewDuration != null
                                                            && x.Shipping != null
                                                            && x.Shipping.Status == ShippingState.ShippingRequestSent
                                                            && x.SentAt != null
                                                            && x.ConfirmedAt == null
                                                            && x.RejectedAt == null)
                                                   .ToListAsync();
                var shippingIds = allRequests.Where(x => (DateTime.Now - x.SentAt.Value).TotalMinutes > x.Carrier.RequestReviewDuration)
                                             .Select(x => x.ShippingId)
                                             .ToList();

                var shippings = await dataService.GetDbSet<Shipping>().Where(x => shippingIds.Contains(x.Id)).ToListAsync();
                var ordersDict = await dataService.GetDbSet<Order>()
                                                  .Where(x => x.ShippingId != null && shippingIds.Contains(x.ShippingId.Value))
                                                  .GroupBy(x => x.ShippingId.Value)
                                                  .ToDictionaryAsync(x => x.Key, x => x.ToList());

                foreach (var shipping in shippings)
                {
                    ordersDict.TryGetValue(shipping.Id, out List<Order> orders);

                    var carrierId = shipping.CarrierId;
                    var carrierAction = new CarrierShippingAction
                    {
                        Id = Guid.NewGuid(),
                        ShippingId = shipping.Id,
                        CarrierId = carrierId,
                        ActionName = "Истечение срока рассмотрения заявки",
                        ActionTime = DateTime.Now
                    };
                    actionsDbSet.Add(carrierAction);

                    shippingActionService.BaseRejectShippingRequest(shipping, orders);
                    carrierSelectionService.FindAndUpdateCarrier(shipping, orders, carrierId);
                }

                triggersService.Execute(false);

                dataService.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Ошибка поиска просроченных заявок в ТК");
            }
        }
    }
}
