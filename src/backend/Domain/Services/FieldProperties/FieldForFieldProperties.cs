using System.Collections.Generic;

namespace Domain.Services.FieldProperties
{
    public class FieldForFieldProperties
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public bool isHidden { get; set; } // на основе таблицы 
        public bool isReadOnly { get; set; } // безу из атрибута
        
        public Dictionary<string, string> AccessTypes { get; set; }
    }
}