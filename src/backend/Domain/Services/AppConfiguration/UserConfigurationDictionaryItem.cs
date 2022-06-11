using System.Collections.Generic;

namespace Domain.Services.AppConfiguration
{
    public class UserConfigurationDictionaryItem
    {
        public string Name { get; set; }

        public bool CanCreateByForm { get; set; }

        public bool CanImportFromExcel { get; set; }

        public bool CanExportToExcel { get; set; }

        public bool CanDelete { get; set; }

        public bool ShowOnHeader { get; set; }

        public IEnumerable<UserConfigurationGridColumn> Columns { get; set; }
    }
}