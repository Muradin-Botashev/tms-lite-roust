using System;

namespace Domain.Services.History
{
    public class HistoryEntryDto
    {
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
    }
}
