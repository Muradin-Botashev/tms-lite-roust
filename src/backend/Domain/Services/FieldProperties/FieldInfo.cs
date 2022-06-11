using Domain.Enums;
using Domain.Extensions;

namespace Domain.Services.FieldProperties
{
    public class FieldInfo
    {
        public string Name { get; set; }

        public string DisplayNameKey { get; set; }

        public FieldType FieldType { get; set; }

        public FieldType FilterType { get; set; }

        public string ReferenceSource { get; set; }

        public bool ShowRawReferenceValue { get; set; }

        public bool IsDefault { get; set; }

        public bool IsBulkUpdateAllowed { get; set; }

        public bool IsFixedPosition { get; set; }

        public bool IsRequired { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsSortDisabled { get; set; }

        public int OrderNumber { get; set; }

        public int? Decimals { get; set; }

        public int? MaxLength { get; set; }

        public string[] Dependencies { get; set; }

        public EmptyValueOptions EmptyValueOptions { get; set; }
    }
}