using DAL.Services;
using Domain.Persistables;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Common;

namespace Tasks.SystemTasks
{
    [Description("Удаление старых результатов автогруппировки")]
    public class ClearObsoleteAutogroupingsTask : TaskBase<PropertiesBase>, IScheduledTask
    {
        public string Schedule => "0 0 * * *";

        protected override Task Execute(IServiceProvider serviceProvider, PropertiesBase parameters, CancellationToken cancellationToken)
        {
            var dataService = serviceProvider.GetService<ICommonDataService>();

            var timeBarrier = DateTime.Today.AddDays(-7);

            var costsDbSet = dataService.GetDbSet<AutogroupingCost>();
            var costEntries = costsDbSet.Where(x => x.CreatedAt < timeBarrier).ToList();
            costsDbSet.RemoveRange(costEntries);

            var ordersDbSet = dataService.GetDbSet<AutogroupingOrder>();
            var orderEntries = ordersDbSet.Where(x => x.CreatedAt < timeBarrier).ToList();
            ordersDbSet.RemoveRange(orderEntries);

            var shippingsDbSet = dataService.GetDbSet<AutogroupingShipping>();
            var shippingEntries = shippingsDbSet.Where(x => x.CreatedAt < timeBarrier).ToList();
            shippingsDbSet.RemoveRange(shippingEntries);

            dataService.SaveChanges();

            return Task.CompletedTask;
        }
    }
}
