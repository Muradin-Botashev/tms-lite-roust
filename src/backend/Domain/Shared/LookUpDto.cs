using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Shared
{
    public class LookUpDto
    {
        public const string EmptyValue = "###EMPTY###";

        public string Value { get; set; }
        public string Name { get; set; }
        public bool IsFilterOnly { get; set; }
        public bool IsBulkUpdateOnly { get; set; }

        public LookUpDto() { }

        public LookUpDto(string value) : this(value, value) { }

        public LookUpDto(string value, string name)
        {
            Value = value;
            Name = name;
        }
    }
}
