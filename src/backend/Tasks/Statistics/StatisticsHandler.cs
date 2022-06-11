using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Tasks.Statistics
{
    public static class StatisticsHandler
    {
        public static async Task Execute(HttpContext context)
        {
            await context.Response.WriteAsync(StatisticsStore.GetCurrentData());
        }
    }
}
