using System.Collections.Generic;

namespace Domain.Shared
{
    public class BulkUpdateDto
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public string Source { get; set; }

        public IEnumerable<string> Ids { get; set; }
    }
}
