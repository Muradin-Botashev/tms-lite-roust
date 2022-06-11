using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Shared;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Application.Shared.Excel.Columns
{
    public class ArrayExcelColumn<TEntity> : IExcelColumn
        where TEntity : class, IPersistable
    {
        private readonly ICommonDataService _dataService;
        private readonly Func<TEntity, string> _getNameMethod;

        private Dictionary<string, Guid> _lookupByName = null;
        private Dictionary<Guid, string> _lookupById = null;

        public ArrayExcelColumn(
            ICommonDataService dataService,
            Func<TEntity, string> getNameMethod)
        {
            _dataService = dataService;
            _getNameMethod = getNameMethod;
        }

        public PropertyInfo Property { get; set; }
        public Domain.Services.FieldProperties.FieldInfo Field { get; set; }
        public string Title { get; set; }
        public int ColumnIndex { get; set; }

        public bool IsValidationEnabled => false;

        public void FillValue(object entity, ExcelRange cell)
        {
            EnsureValues();
            var dtos = Property.GetValue(entity) as IEnumerable<LookUpDto>;
            if (dtos != null && dtos.Any())
            {
                cell.Value = string.Join("; ", dtos.Select(GetName));
            }
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
                EnsureValues();

                var cellValues = cellValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(x => GetValue(x.Trim()))
                                          .Where(x => x != null)
                                          .ToList();

                Property.SetValue(entity, cellValues);
            }

            return null;
        }

        private void EnsureValues()
        {
            if (_lookupByName == null)
            {
                _lookupByName = new Dictionary<string, Guid>();
                _lookupById = new Dictionary<Guid, string>();
                var entities = _dataService.GetDbSet<TEntity>().ToList();
                foreach (var entity in entities)
                {
                    var name = _getNameMethod(entity);
                    if (!string.IsNullOrEmpty(name))
                    {
                        _lookupByName[name] = entity.Id;
                        _lookupById[entity.Id] = name;
                    }
                }
            }
        }

        private string GetName(LookUpDto dto)
        {
            var id = dto?.Value.ToGuid();
            if (id != null && _lookupById.TryGetValue(id.Value, out string result))
            {
                return result;
            }
            else
            {
                return string.Empty;
            }
        }

        private LookUpDto GetValue(string name)
        {
            if (!string.IsNullOrEmpty(name) && _lookupByName.TryGetValue(name, out Guid id))
            {
                return new LookUpDto(id.FormatGuid(), name);
            }
            else
            {
                return null;
            }
        }
    }
}
