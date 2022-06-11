using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks.Common
{
    public interface IScheduledTask
    {
        string Schedule { get; }
        string TaskName { get; }
        Task Execute(IServiceProvider serviceProvider, string consoleParameters, CancellationToken cancellationToken);
    }
}
