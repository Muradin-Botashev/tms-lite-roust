using Domain.Services;
using System.Collections.Generic;

namespace Domain.Shared
{
    public class OperationDetailedResult: AppResult
    {
        public string Error { get; set; }

        public List<OperationDetailedResultItem> Entries { get; set; }

        public OperationDetailedResult()
        {
            IsError = false;
            Error = null;
            Message = null;
            Entries = new List<OperationDetailedResultItem>();
            MessageType = AppResultType.DetailedNotification;
        }
    }
}
