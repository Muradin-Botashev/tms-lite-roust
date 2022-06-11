using System.Collections.Generic;

namespace Domain.Services.AppConfiguration
{
    public class AppConfigurationDto
    {
        public IEnumerable<UserConfigurationGridItem> Grids { get; set; }
        public IEnumerable<UserConfigurationDictionaryItem> Dictionaries { get; set; }
        public bool EditUsers { get; set; }
        public bool EditRoles { get; set; }
        public bool EditFieldProperties { get; set; }
        public bool EditAutogroupingSettings { get; set; }
        public bool ImportShippingVehicle { get; set; }
        public bool ImportOrders { get; set; }
        public bool AutogroupingOrders { get; set; }
        public bool ViewOperationalReport { get; set; }
        public bool ViewRegistryReport { get; set; }
        public bool PoolingWarehousesImport { get; set; }
        public bool InvoiceImport { get; set; }
    }
}