using System.Collections.Generic;

namespace Domain.Shared
{
    public class OperationDetailedResultItem
    {
        public bool IsError { get; set; }
        public string Title { get; set; }
        public int MessageColumns { get; set; }
        public List<string> Messages { get; set; }

        public OperationDetailedResultItem()
        {
            IsError = false;
            Title = null;
            MessageColumns = 1;
            Messages = new List<string>();
        }

        public OperationDetailedResultItem(string title) : this()
        {
            Title = title;
        }
    }
}
