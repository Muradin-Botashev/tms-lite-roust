using System.Collections.Generic;

namespace Domain.Shared.FormFilters
{
    public class SearchFilterDto
    {
        public string Search { get; set; }

        public List<string> Columns {get; set; }
    }
}