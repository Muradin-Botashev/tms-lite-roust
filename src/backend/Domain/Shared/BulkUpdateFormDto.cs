using System.Collections.Generic;

namespace Domain.Shared
{
    public class BulkUpdateFormDto
    {
        public IEnumerable<string> Ids { get; set; }
        public string Value { get; set; }
    }
}
