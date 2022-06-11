using DAL.Services;
using Domain.Services;
using Domain.Services.History;

namespace Application.Shared
{
    public class ChangeTrackerFactory : IChangeTrackerFactory
    {
        private readonly ICommonDataService _dataService;

        private readonly IHistoryService _historyService;

        public ChangeTrackerFactory(ICommonDataService dataService, IHistoryService historyService)
        {
            _dataService = dataService;
            _historyService = historyService;
        }

        public IChangeTracker CreateChangeTracker()
        {
            return new ChangeTracker(_dataService, _historyService);
        }
    }
}
