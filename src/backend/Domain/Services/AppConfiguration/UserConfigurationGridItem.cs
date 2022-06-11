using System.Collections.Generic;

namespace Domain.Services.AppConfiguration
{
    public class UserConfigurationGridItem
    {
        public string Name { get; set; }
        public bool CanCreateByForm { get; set; }
        public bool CanViewAdditionSummary { get; set; }
        public bool CanImportFromExcel { get; set; }
        public bool CanExportToExcel { get; set; }
        public IEnumerable<UserConfigurationGridColumn> Columns { get; set; }
    }
}