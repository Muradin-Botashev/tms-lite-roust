using System.Collections.Generic;

namespace Domain.Shared
{
    public class ExportExcelFormDto<TFilter> : FilterFormDto<TFilter>
    {
        public List<string> Columns { get; set; }
    }
}
