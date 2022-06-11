using Domain.Extensions;
using Domain.Services.FieldProperties;
using System.Linq;

namespace Domain.Services.AppConfiguration
{
    public class UserConfigurationGridColumn
    {
        public UserConfigurationGridColumn(FieldInfo field)
        {
            Name = field.Name.ToLowerFirstLetter();
            DisplayNameKey = field.DisplayNameKey;
            Type = field.FieldType.ToString();
            FilterType = field.FilterType.ToString();
            IsDefault = field.IsDefault;
            IsFixedPosition = field.IsFixedPosition;
            IsRequired = field.IsRequired;
            IsReadOnly = field.IsReadOnly;
            IsSortDisabled = field.IsSortDisabled;
            Decimals = field.Decimals;
            MaxLength = field.MaxLength;
            EmptyValue = field.EmptyValueOptions.FormatEnum();
        }

        public string Name { get; set; }
        public string DisplayNameKey { get; set; }
        public string Type { get; set; }
        public string FilterType { get; set; }
        public bool IsDefault { get; set; }
        public bool IsFixedPosition { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSortDisabled { get; set; }
        public int? Decimals { get; set; }
        public int? MaxLength { get; set; }
        public string EmptyValue { get; set; }
    }

    public class UserConfigurationGridColumnWhitchSource : UserConfigurationGridColumn
    {
        public string Source { get; }
        public bool ShowRawValue { get; set; }
        public string[] Dependencies { get; set; }

        public UserConfigurationGridColumnWhitchSource(FieldInfo field)
            : base(field)
        {
            Source = field.ReferenceSource.Replace("Service", "").ToLowerFirstLetter();
            ShowRawValue = field.ShowRawReferenceValue;
            Dependencies = field.Dependencies?.Select(x => x.ToLowerFirstLetter()).ToArray();
        }
    }
}