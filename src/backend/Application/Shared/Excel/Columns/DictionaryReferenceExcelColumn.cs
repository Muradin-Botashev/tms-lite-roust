using DAL.Services;
using Domain.Persistables;
using Domain.Shared.UserProvider;
using Domain.Shared;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FieldInfo = Domain.Services.FieldProperties.FieldInfo;

namespace Application.Shared.Excel.Columns
{
    public class DictionaryReferenceExcelColumn<TEntity> : IExcelColumn
        where TEntity : class, IPersistable, ICompanyPersistable
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly Func<TEntity, string> _getNameMethod;

        private Dictionary<string, Guid> _values = null;

        public DictionaryReferenceExcelColumn(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            Func<TEntity, string> getNameMethod,
            bool isValidationEnabled = false)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _getNameMethod = getNameMethod;
            IsValidationEnabled = isValidationEnabled;
        }

        public PropertyInfo Property { get; set; }
        public FieldInfo Field { get; set; }
        public string Title { get; set; }
        public int ColumnIndex { get; set; }

        public bool IsValidationEnabled { get; private set; }

        public void FillValue(object entity, ExcelRange cell)
        {
            string name = (Property.GetValue(entity) as LookUpDto)?.Name;
            cell.Value = name;
        }

        public ValidationResultItem SetValue(object entity, ExcelRange cell)
        {
            string refName = cell.GetValue<string>()?.Trim();
            string refId = string.IsNullOrEmpty(refName) ? null : GetIdByName(refName)?.ToString();

            if (!string.IsNullOrEmpty(refName) && string.IsNullOrEmpty(refId))
            {
                return new ValidationResultItem
                {
                    Name = refName,
                    Message = "invalidDictionaryValue",
                    ResultType = ValidationErrorType.InvalidDictionaryValue
                };
            }

            Property.SetValue(entity, new LookUpDto(refId, refName));

            return null;
        }

        public List<string> GetPossibleValues()
        {
            EnsureValues();
            return _values.Keys.OrderBy(x => x).ToList();
        }

        private Guid? GetIdByName(string name)
        {
            EnsureValues();
            if (!string.IsNullOrEmpty(name) && _values.TryGetValue(name, out Guid result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private void EnsureValues()
        {
            if (_values == null)
            {
                var companyId = _userProvider.GetCurrentUser()?.CompanyId;
                var entities = _dataService.GetDbSet<TEntity>()
                                           .Where(x => x.CompanyId == null || companyId == null || x.CompanyId == companyId)
                                           .ToList();

                _values = new Dictionary<string, Guid>();
                foreach (var entity in entities)
                {
                    var name = _getNameMethod(entity);
                    if (!string.IsNullOrEmpty(name))
                    {
                        _values[name] = entity.Id;
                    }
                }
            }
        }
    }
}
