using Application.Shared.Excel.Columns;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.Excel
{
    public class ExcelMapper<TDto> where TDto: new()
    {
        public void AddColumn(IExcelColumn column)
        {
            string columnKey = GetColumnKey(column);
            column.Field = _fieldDispatcherService.GetDtoFields<TDto>().Where(x => x.Name == column.Property.Name).FirstOrDefault();
            _columns[columnKey] = column;
        }

        public void RemoveColumn(IExcelColumn column)
        {
            string columnKey = GetColumnKey(column);
            if (_columns.ContainsKey(columnKey))
            {
                _columns.Remove(columnKey);
            }
        }

        public void RemoveColumn(System.Reflection.PropertyInfo property)
        {
            string columnKey = GetColumnKey(property);
            if (_columns.ContainsKey(columnKey))
            {
                _columns.Remove(columnKey);
            }
        }

        public void FillSheet(ExcelWorksheet worksheet, IEnumerable<TDto> entries, string lang, 
                              List<string> columns = null, Func<TDto, bool> hasBacklight = null)
        {
            FillDefaultColumnOrder(columns);
            FillColumnTitles(lang);

            foreach (var column in _columns.Values.Where(c => c.ColumnIndex >= 0))
            {
                worksheet.Cells[1, column.ColumnIndex].Value = column.Title;
                worksheet.Cells[1, column.ColumnIndex].Style.Font.Bold = true;
            };

            FillSheetDataInner(worksheet, entries, lang, hasBacklight);
        }

        public void FillSheetData(ExcelWorksheet worksheet, IEnumerable<TDto> entries, string lang, 
                                  List<string> columns = null, Func<TDto, bool> hasBacklight = null, int titleRowsCount = 1)
        {
            FillDefaultColumnOrder(columns);
            FillSheetDataInner(worksheet, entries, lang, hasBacklight, titleRowsCount);
        }

        public void FillSheetDataInner(ExcelWorksheet worksheet, IEnumerable<TDto> entries, string lang, 
                                       Func<TDto, bool> hasBacklight, int titleRowsCount = 1)
        {
            int maxColumnIndex = Math.Max(1, _columns.Values.Max(c => c.ColumnIndex));

            int rowIndex = titleRowsCount;
            foreach (var entry in entries)
            {
                ++rowIndex;
                foreach (var column in _columns.Values.Where(c => c.ColumnIndex >= 0))
                {
                    var cell = worksheet.Cells[rowIndex, column.ColumnIndex];
                    column.FillValue(entry, cell);
                };

                bool isHighlighted = hasBacklight != null && hasBacklight(entry);
                if (isHighlighted)
                {
                    worksheet.Cells[rowIndex, 1, rowIndex, maxColumnIndex].Style.Font.Bold = true;
                }
            }

            foreach (var column in _columns.Values.Where(c => c.ColumnIndex >= 0))
            {
                var possibleValues = column.GetPossibleValues();
                if (possibleValues != null && possibleValues.Any())
                {
                    FillDictionaryValidation(worksheet, column, titleRowsCount + 1, possibleValues, lang);
                }
            }
        }

        public IEnumerable<ValidatedRecord<TDto>> LoadEntries(ExcelWorksheet worksheet, string lang)
        {
            var rows = worksheet.Cells
                .Select(cell => cell.Start.Row)
                .Distinct()
                .OrderBy(x => x);

            if (!rows.Any())
            {
                yield break;
            }

            int headRowIndex = rows.First();
            int maxColumnInd = worksheet.Cells.Select(c => c.End.Column).Max();
            List<string> columnTitles = new List<string>();
            for (int colIndex = 1; colIndex <= maxColumnInd; colIndex++)
            {
                columnTitles.Add(worksheet.Cells[headRowIndex, colIndex]?.Value?.ToString());
            }

            columnTitles = Unlocalize(columnTitles, _columns.Values.Select(x => x.Field)).ToList();

            FillColumnOrder(columnTitles);

            foreach(int rowIndex in rows.Skip(1))
            {
                bool isEmpty = IsEmptyRow(worksheet, rowIndex);
                if (isEmpty)
                {
                    continue;
                }

                var entity = new TDto();
                var validationResult = new DetailedValidationResult();

                foreach (var column in _columns)
                {
                    try
                    {
                        var cell = worksheet.Cells[rowIndex, column.Value.ColumnIndex];
                        var columnResult = column.Value.SetValue(entity, cell);

                        if (columnResult != null)
                        { 
                            validationResult.AddError(column.Key, 
                                                      "importLineError".Translate(lang, rowIndex, columnResult.Message), 
                                                      columnResult.ResultType);
                        }

                    }
                    catch (Exception ex)
                    {
                        validationResult.AddError("exception", 
                                                  "importLineError".Translate(lang, rowIndex, ex.Message), 
                                                  ValidationErrorType.Exception);
                    }
                };

                _errors.Add(validationResult);

                yield return new ValidatedRecord<TDto>(entity, rowIndex, validationResult);
            }
        }

        public IEnumerable<ValidateResult> Errors => _errors;

        public IEnumerable<IExcelColumn> Columns => _columns.Values;

        private void FillColumnTitles(string lang)
        {
            foreach (var column in _columns.Where(c => c.Value.ColumnIndex >= 0))
            {
                Translation local = _translations.FirstOrDefault(t => t.Name == column.Value.Field.DisplayNameKey);
                column.Value.Title = (lang == "en" ? local?.En : local?.Ru) ?? column.Key;
            }
        }

        private void FillDefaultColumnOrder(List<string> columns)
        {
            if (columns != null && columns.Any())
            {
                List<string> propNames = columns.Select(s => s.ToLower()).ToList();
                foreach (var column in _columns)
                {
                    column.Value.ColumnIndex = propNames.IndexOf(column.Key);
                }
            }
            else
            {
                foreach (var column in _columns)
                {
                    column.Value.ColumnIndex = column.Value.Field.OrderNumber;
                }
            }

            int columnIndex = 0;
            foreach (var column in _columns.Values.Where(c => c.ColumnIndex >= 0).OrderBy(c => c.ColumnIndex))
            {
                ++columnIndex;
                column.ColumnIndex = columnIndex;
            }
        }

        private void FillColumnOrder(List<string> columnTitles)
        {
            foreach (var columnKey in _columns.Keys.ToList())
            {
                int colInd = columnTitles.IndexOf(columnKey);
                if (colInd >= 0)
                {
                    IExcelColumn column = _columns[columnKey];
                    column.ColumnIndex = colInd + 1;
                }
                else
                {
                    _columns.Remove(columnKey);
                }
            }
        }

        private void FillDictionaryValidation(ExcelWorksheet dataSheet, IExcelColumn column, int startRowIndex, List<string> values, string lang)
        {
            var refSheetName = $"ref__{column.Property.Name}";
            if (!dataSheet.Workbook.Worksheets.Any(x => x.Name == refSheetName))
            {
                var refSheet = dataSheet.Workbook.Worksheets.Add(refSheetName);
                refSheet.Hidden = eWorkSheetHidden.VeryHidden;
                for (int i = 0; i < values.Count; i++)
                {
                    refSheet.Cells[i + 1, 1].Value = values[i];
                }
            }

            var validation = dataSheet.DataValidations.AddListValidation(dataSheet.Cells[startRowIndex, column.ColumnIndex, 10000, column.ColumnIndex].Address);
            validation.ShowErrorMessage = column.IsValidationEnabled;
            validation.ErrorStyle = column.IsValidationEnabled ? ExcelDataValidationWarningStyle.warning : ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = "listValidationTitle".Translate(lang);
            validation.Error = "listValidationMessage".Translate(lang);
            validation.Formula.ExcelFormula = $"{refSheetName}!A:A";
        }

        private IEnumerable<string> Unlocalize(IEnumerable<string> titles, IEnumerable<FieldInfo> fields)
        {
            var fieldNamesSet = fields.ToDictionary(x => x.DisplayNameKey);
            foreach (string title in titles)
            {
                if (string.IsNullOrEmpty(title))
                {
                    yield return string.Empty;
                }
                else
                {
                    Translation local = _translations.FirstOrDefault(t => (t.Ru == title || t.En == title) && fieldNamesSet.ContainsKey(t.Name));
                    if (local == null)
                    {
                        yield return title?.ToLower();
                    }
                    else
                    {
                        yield return fieldNamesSet[local.Name].Name.ToLower();
                    }
                }
            }
        }

        private string GetColumnKey(IExcelColumn column)
        {
            return GetColumnKey(column.Property);
        }

        private string GetColumnKey(System.Reflection.PropertyInfo property)
        {
            return property.Name.ToLower();
        }

        private bool IsEmptyRow(ExcelWorksheet worksheet, int rowIndex)
        {
            foreach (var column in _columns.Values)
            {
                if (column.ColumnIndex >= 0)
                {
                    var value = worksheet.Cells[rowIndex, column.ColumnIndex];
                    var strValue = value.Value?.ToString();
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void InitColumns()
        {
            Type type = typeof(TDto);
            string lang = _userProvider.GetCurrentUser()?.Language;

            _fieldDispatcherService.GetDtoFields<TDto>()
                    .Where(f => f.FieldType != FieldType.Enum && f.FieldType != FieldType.State)
                    .Select(f => new BaseExcelColumn { Property = type.GetProperty(f.Name), Field = f, Language = lang })
                    .ToList()
                    .ForEach(AddColumn);
        }

        public ExcelMapper(ICommonDataService dataService, IUserProvider userProvider, IFieldDispatcherService fieldDispatcherService)
        {
            _userProvider = userProvider;
            _fieldDispatcherService = fieldDispatcherService;
            _translations = dataService.GetDbSet<Translation>().ToList();
            InitColumns();
        }

        private readonly IUserProvider _userProvider;
        private readonly IFieldDispatcherService _fieldDispatcherService;
        private readonly List<Translation> _translations;
        private readonly Dictionary<string, IExcelColumn> _columns = new Dictionary<string, IExcelColumn>();
        private readonly List<ValidateResult> _errors = new List<ValidateResult>();
    }
}
