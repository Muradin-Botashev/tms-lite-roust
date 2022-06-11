using System.Collections.Generic;

namespace Domain.Services.Identity
{
    public class PermissionsData
    {
        public IEnumerable<string> Permissions { get; set; }
        public bool IsLogged { get; set; }
        public string FieldsConfig { get; set; }
    }
}