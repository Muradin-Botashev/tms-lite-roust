using Domain.Enums;
using Domain.Extensions;
using Domain.Services.Translations;
using Domain.Shared;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Reflection;
using FieldInfo = Domain.Services.FieldProperties.FieldInfo;

namespace Application.Shared.Excel.Columns
{
    public class BaseExcelColumn : IExcelColumn
    {
        public PropertyInfo Property { get; set; }
        public FieldInfo Field { get; set; }
        public string Title { get; set; }
        public int ColumnIndex { get; set; }
        public string Language { get; set; }

        public bool IsValidationEnabled => false;

        public void FillValue(object entity, ExcelRange cell)
        {
            if (Property.PropertyType == typeof(DateTime?) && Field.FieldType == FieldType.Date)
            {
                cell.Style.Numberformat.Format = "dd/MM/yyyy";
                var dateTime = (DateTime?)Property.GetValue(entity);
                if (dateTime.HasValue)
                    cell.Value = dateTime.Value.ToOADate();
            }
            else if (Property.PropertyType == typeof(DateTime?))
            {
                var numberformatFormat = "dd.MM.yyyy HH:mm:ss";

                cell.Style.Numberformat.Format = numberformatFormat;
                var dateTime = (DateTime?)Property.GetValue(entity);
                if (dateTime.HasValue)
                    cell.Value = dateTime.Value.ToString(numberformatFormat);
            }
            else if (Property.PropertyType == typeof(bool) || Property.PropertyType == typeof(bool?))
            {
                bool? value = (bool?)Property.GetValue(entity);
                if (value.HasValue)
                {
                    cell.Value = (value == true ? "Yes" : "No").Translate(Language);
                }
            }
            else if (Property.PropertyType == typeof(LookUpDto))
            {
                LookUpDto value = (LookUpDto)Property.GetValue(entity);
                if (value != null)
                {
                    cell.Value = value.Value;
                }
            }
            else if (Property.PropertyType == typeof(ValidatedNumberDto) 
                    || Property.PropertyType == typeof(Domain.Services.Autogrouping.RouteCostDto))
            {
                ValidatedNumberDto value = (ValidatedNumberDto)Property.GetValue(entity);
                if (value != null)
                {
                    cell.Value = value.Value;
                }
            }
            else if (Property.PropertyType == typeof(decimal) || Property.PropertyType == typeof(decimal?))
            {
                decimal? value = (decimal?)Property.GetValue(entity);
                if (value.HasValue)
                {
                    RoundAttribute roundAttr = Property.GetCustomAttribute<RoundAttribute>();
                    if (roundAttr != null)
                    {
                        value = decimal.Round(value.Value, roundAttr.Decimals);
                    }
                    cell.Value = value;
                }
            }
            else
            {
                cell.Value = Property.GetValue(entity);
            }
        }

        public List<string> GetPossibleValues()
        {
            return null;
        }

        public ValidationResultItem SetValue(object entity, ExcelRange cell)
        {
            if (cell.Value == null)
            {
                Property.SetValue(entity, null);
            }
            else if (Property.PropertyType == typeof(int) || Property.PropertyType == typeof(int?))
            {
                return FillPropertyValue<int?>(entity, cell, "invalidIntegerValueFormat");
            }
            else if (Property.PropertyType == typeof(decimal) || Property.PropertyType == typeof(decimal?))
            {
                return FillPropertyValue<decimal?>(entity, cell, "invalidDecimalValueFormat");
            }
            else if (Property.PropertyType == typeof(DateTime) || Property.PropertyType == typeof(DateTime?))
            {
                return FillPropertyValue<DateTime?>(entity, cell, "invalidDateValueFormat");
            }
            else if (Property.PropertyType == typeof(bool) || Property.PropertyType == typeof(bool?))
            {
                bool? value = null;
                string cellValue = cell.GetValue<string>()?.ToLower()?.Trim();
                if (cellValue == "да" || cellValue == "д" || cellValue == "yes" || cellValue == "y")
                {
                    value = true;
                }
                else if (cellValue == "нет" || cellValue == "н" || cellValue == "no" || cellValue == "n")
                {
                    value = false;
                }

                if (Property.PropertyType == typeof(bool))
                {
                    Property.SetValue(entity, value ?? false);
                }
                else
                {
                    Property.SetValue(entity, value);
                }
            }
            else if (Field.FieldType == FieldType.Time)
            {
                var rawValue = cell.Value?.ToString()?.Trim();
                if (cell.Value is DateTime)
                {
                    var date = (DateTime)cell.Value;
                    Property.SetValue(entity, date.TimeOfDay.FormatTime());
                }
                else if (cell.Value is double)
                {
                    var dateNumber = (double)cell.Value;
                    try
                    {
                        var date = DateTime.FromOADate(dateNumber);
                        Property.SetValue(entity, date.TimeOfDay.FormatTime());
                    }
                    catch
                    {
                        return new ValidationResultItem
                        {
                            Message = "invalidTimeValueFormat".Translate(Language, rawValue),
                            ResultType = ValidationErrorType.InvalidValueFormat
                        };
                    }
                }
                else
                {
                    Property.SetValue(entity, cell.Value?.ToString());
                }
            }
            else
            if (Field.FieldType == FieldType.Date || Field.FieldType == FieldType.DateTime || Field.FieldType == FieldType.LocalDateTime)
            {
                var rawValue = cell.Value?.ToString()?.Trim();
                if (cell.Value is DateTime)
                {
                    var date = (DateTime)cell.Value;
                    Property.SetValue(entity, date.FormatDate());
                }
                else if (cell.Value is double)
                {
                    var dateNumber = (double)cell.Value;
                    try
                    {
                        var date = DateTime.FromOADate(dateNumber);
                        Property.SetValue(entity, date.FormatDate());
                    }
                    catch
                    {
                        return new ValidationResultItem
                        {
                            Message = "invalidDateValueFormat".Translate(Language, rawValue),
                            ResultType = ValidationErrorType.InvalidValueFormat
                        };
                    }
                }
                else
                {
                    Property.SetValue(entity, cell.Value?.ToString());
                }
            }
            else if (Property.PropertyType == typeof(LookUpDto))
            {
                Property.SetValue(entity, new LookUpDto(cell.Value?.ToString()?.Trim()));
            }
            else if (Property.PropertyType == typeof(ValidatedNumberDto))
            {
                var numberValue = cell.GetValue<decimal?>();
                if (numberValue != null)
                {
                    Property.SetValue(entity, new ValidatedNumberDto { Value = numberValue });
                }
            }
            else
            {
                Property.SetValue(entity, cell.Value?.ToString()?.Trim());
            }

            return null;
        }

        private ValidationResultItem FillPropertyValue<T>(object entity, ExcelRange cell, string formatErrorKey = "InvalidValueFormat")
        {
            T value;
            try
            {
                value = cell.GetValue<T>();
            }
            catch
            {
                string rawValue = cell.Value?.ToString();
                return new ValidationResultItem
                {
                    Message = formatErrorKey.Translate(Language, rawValue),
                    ResultType = ValidationErrorType.InvalidValueFormat
                };
            }

            Property.SetValue(entity, value);
            return null;
        }
    }
}
