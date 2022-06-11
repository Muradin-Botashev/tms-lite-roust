using Application.Shared.Excel.Columns;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Shared.Excel
{
    public static class ExcelMapperExtensions
    {
        public static ExcelMapper<TDto> MapColumn<TDto, TField>(this ExcelMapper<TDto> excelMapper, Expression<Func<TDto, TField>> property, IExcelColumn column)
            where TDto : new()
        {
            var propertyBody = property.Body as MemberExpression;
            if (propertyBody != null)
            {
                var propertyInfo = propertyBody.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    column.Property = propertyInfo;
                    excelMapper.AddColumn(column);
                }
            }
            return excelMapper;
        }

        public static ExcelMapper<TDto> RemoveColumn<TDto, TField>(this ExcelMapper<TDto> excelMapper, Expression<Func<TDto, TField>> property)
            where TDto : new()
        {
            var propertyBody = property.Body as MemberExpression;
            if (propertyBody != null)
            {
                var propertyInfo = propertyBody.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    excelMapper.RemoveColumn(propertyInfo);
                }
            }
            return excelMapper;
        }
    }
}
