using Domain.Enums;
using System;

namespace Domain.Extensions
{
    public class FieldTypeAttribute : Attribute
    {
        public FieldType Type { get; set; }

        public FieldType FilterType { get; set; }

        public string Source { get; set; }

        public bool ShowRawValue { get; set; }
        public string[] Dependencies { get; set; }

        public EmptyValueOptions EmptyValue { get; set; } = EmptyValueOptions.NotAllowed;

        public FieldTypeAttribute(FieldType type, string source = null, bool showRawValue = false, string[] dependencies = null)
        {
            Type = type;
            FilterType = type;
            Source = source;
            ShowRawValue = showRawValue;
            Dependencies = dependencies;
        }

        public FieldTypeAttribute(FieldType type, FieldType filterType, string source = null, bool showRawValue = false, string[] dependencies = null)
        {
            Type = type;
            FilterType = filterType;
            Source = source;
            ShowRawValue = showRawValue;
            Dependencies = dependencies;
        }
    }

    public enum EmptyValueOptions
    {
        NotAllowed,
        FilterOnly,
        Allowed
    }
}