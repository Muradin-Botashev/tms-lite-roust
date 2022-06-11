using Domain.Services.Translations;
using Domain.Shared;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Application.Shared.Excel.Columns
{
    public class EnumArrayExcelColumn<TEnum> : IExcelColumn
    {
        private readonly string _lang;

        public EnumArrayExcelColumn(string lang)
        {
            _lang = lang;
        }

        public PropertyInfo Property { get; set; }
        public Domain.Services.FieldProperties.FieldInfo Field { get; set; }
        public string Title { get; set; }
        public int ColumnIndex { get; set; }

        public bool IsValidationEnabled => false;

        public void FillValue(object entity, ExcelRange cell)
        {
            var values = Property.GetValue(entity) as IEnumerable<LookUpDto>;
            cell.Value = string.Join(", ", values.Select(x => x.Value?.Translate(_lang)));
        }

        public List<string> GetPossibleValues()
        {
            return null;
        }

        public ValidationResultItem SetValue(object entity, ExcelRange cell)
        {
            string cellValue = cell.GetValue<string>()?.Trim();
            if (string.IsNullOrEmpty(cellValue))
            {
                Property.SetValue(entity, null);
            }
            else
            {
                List<string> valueNames = new List<string>();
                foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
                {
                    valueNames.Add(value.ToString());
                }

                var cellValues = cellValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(x => GetValidCellValue(x.Trim(), valueNames))
                                          .Where(x => !string.IsNullOrEmpty(x))
                                          .Select(x => new LookUpDto(x))
                                          .ToList();

                Property.SetValue(entity, cellValues);
            }

            return null;
        }

        private string GetValidCellValue(string cellValue, List<string> valueNames)
        {
            var keys = TranslationProvider.GetKeysByTranslation(cellValue);
            keys = keys.Select(x => x.ToLower());
            string validCellValue = keys.FirstOrDefault(x => valueNames.Any(y => string.Compare(x, y, true) == 0));
            if (string.IsNullOrEmpty(validCellValue))
            {
                string lowerCellValue = cellValue.ToLower();
                validCellValue = valueNames.FirstOrDefault(n => n.ToLower() == lowerCellValue);
            }
            return validCellValue;
        }
    }
}
