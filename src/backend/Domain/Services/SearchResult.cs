using System.Collections.Generic;

namespace Domain.Services
{
    public class SearchResult<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}