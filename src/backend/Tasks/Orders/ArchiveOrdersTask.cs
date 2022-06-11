using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Common;

namespace Tasks.Orders
{
    [Description("Перевод заказов в Архив")]
    public class ArchiveOrdersTask : TaskBase<ArchiveOrdersTaskProperties>, IScheduledTask
    {
        public string Schedule { get; set; }

        protected override async Task Execute(IServiceProvider serviceProvider, ArchiveOrdersTaskProperties parameters, CancellationToken cancellationToken)
        {
            int.TryParse(parameters.ExpirationPeriod, out var expirationPeriod);

            Log.Information($"Запуск поиска архивных заказов старше {expirationPeriod} часов");

            var dataService = serviceProvider.GetService<ICommonDataService>();
            var historyService = serviceProvider.GetService<IHistoryService>();


            var ordersSelfDelivery = dataService.GetDbSet<Order>()
                  .Where(i => i.Status == OrderState.Shipped)
                  .Where(i => i.DeliveryType == DeliveryType.SelfDelivery)
                  .Where(i => i.StatusChangedAt <= DateTime.Now.AddHours(-expirationPeriod));

            var ordersCourier = dataService.GetDbSet<Order>()
                  .Where(i => i.Status == OrderState.Delivered)
                  .Where(i => i.DeliveryType == DeliveryType.Courier)
                  .Where(i => i.StatusChangedAt <= DateTime.Now.AddHours(-expirationPeriod));

            var orders = await ordersSelfDelivery.Union(ordersCourier).ToListAsync();

            foreach (var order in orders)
            {
                order.Status = OrderState.Archive;
                historyService.Save(order.Id, "orderSetArchived", order.OrderNumber);
            }

            dataService.SaveChanges();
        }
    }
}
