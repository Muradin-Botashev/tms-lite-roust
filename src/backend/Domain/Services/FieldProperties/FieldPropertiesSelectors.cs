using System.Collections.Generic;

namespace Domain.Services.FieldProperties
{
    public class FieldPropertiesSelectors
    {
        public IEnumerable<FieldMatrixSelectorItem> Companies { get; set; }
        public IEnumerable<FieldMatrixSelectorItem> Roles { get; set; }

        public string CompanyId { get; set; }
        public string RoleId { get; set; }        
    }
}