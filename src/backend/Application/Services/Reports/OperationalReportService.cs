using Application.Extensions;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Persistables.Queries;
using Domain.Services.AppConfiguration;
using Domain.Services.FieldProperties;
using Domain.Services.Reports;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Application.Services.Reports
{
    public class OperationalReportService : IReportService
    {
        private readonly ICommonDataService _dataService;

        private readonly IUserProvider _userProvider;

        private readonly IFieldDispatcherService _fieldDispatcherService;

        public OperationalReportService(ICommonDataService dataService, IUserProvider userProvider, IFieldDispatcherService fieldDispatcherService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _fieldDispatcherService = fieldDispatcherService;
        }

        public Stream ExportReport(ReportParametersDto filter)
        {
            var fileName = GetSheetName(filter);

            var excel = new ExcelPackage();

            var workSheet = excel.Workbook.Worksheets.Add(fileName);

            var report = GetReport(filter);

            FillHeaders(workSheet, report);
            FillData(workSheet, report);

            return new MemoryStream(excel.GetAsByteArray());
        }

        private void FillHeaders(ExcelWorksheet worksheet, ReportResultDto data)
        {
            var index = 1;

            var user = _userProvider.GetCurrentUser();

            foreach (var column in data.Columns)
            {
                worksheet.Cells[1, index].Value = column.Translate(user.Language);
                worksheet.Cells[1, index++].Style.Font.Bold = true;
            }
        }

        private void FillData(ExcelWorksheet worksheet, ReportResultDto data)
        {
            var props = typeof(OrderReportDto).GetProperties().ToDictionary(i => i.Name, i => i);

            var user = _userProvider.GetCurrentUser();

            var rowIndex = 2;
            
            foreach (var row in data.Data)
            {
                var columnIndex = 1;

                foreach (var column in data.Columns)
                {
                    var prop = props[column.ToUpperFirstLetter()];
                    worksheet.Cells[rowIndex, columnIndex++].Value = GetValue(row, prop, user.Language);
                }

                rowIndex++;
            }
        }

        private string GetValue(object data, PropertyInfo property, string lang)
        {
            if (property.PropertyType == typeof(DateTime?))
            {
                return ((DateTime?)property.GetValue(data))?.FormatDate();
            }
            else if (property.PropertyType == typeof(LookUpDto))
            {
                return ((LookUpDto)property.GetValue(data))?.Name;
            }

            return property.GetValue(data)?.ToString();
        }

        private string GetSheetName(ReportParametersDto filter)
        {
            DateTime? startDate = filter.StartDate.ToDate();
            DateTime? endDate = filter.EndDate.ToDate();

            if (!startDate.HasValue && !endDate.HasValue)
            {
                return "Отчёт";
            }

            var date = "";

            if (endDate.HasValue && startDate.HasValue && endDate != startDate)
            {
                date = $"{startDate.FormatDate()}_1 - {endDate.FormatDate()}_2";
            }
            else
            {
                date = startDate.HasValue ? startDate.FormatDate() : endDate.FormatDate();
            }

            return $"Отчёт_{date}";
        }

        public ReportResultDto GetReport(ReportParametersDto filter)
        {
            var groupByColumns = GetGroupColumns(filter);

            var selectColumns = GetSelectColumns(filter).Select(i => $@"{i.Value} AS ""{i.Key}""");

            var where = GetWhereClause(filter);

            var sort = GetSortClause(filter);

            DateTime? startDate = filter.StartDate.ToDate();
            DateTime? endDate = filter.EndDate.ToDate();

            var parameters = new List<object>()
            {
                startDate,
                endDate
            };

            var having = GetFilter(filter, ref parameters);

            var sql = $@"SELECT { string.Join(", ", selectColumns) } 
                         FROM ""Orders"" { where } 
                         GROUP BY { string.Join(", ", groupByColumns.Select(i => $@"""{i}""")) }
                         {having}
                         {sort}";

            var result = _dataService.FromSql<OrderReport>(sql, parameters.ToArray()).ToList();

            var columns = new List<string>(groupByColumns);

            columns.InsertRange(groupByColumns.Count() - 1, new[]
            {
                 nameof(OrderReport.OrdersCount),
                 nameof(OrderReport.PalletsCount),
                 nameof(OrderReport.OrderAmountExcludingVAT)
            });

            var user = _userProvider.GetCurrentUser();
            
            return new ReportResultDto
            {
                Columns = columns.Select(i => i.ToLowerFirstLetter()),
                Data = result.Select(i => MapResult(i, user.Language))
            };
        }

        public UserConfigurationGridItem GetReportConfiguration()
        {
            var columns = new List<UserConfigurationGridColumn>();
            var fields = _fieldDispatcherService.GetDtoFields<OrderReportDto>();

            foreach (var field in fields.OrderBy(f => f.OrderNumber))
            {
                if (string.IsNullOrEmpty(field.ReferenceSource))
                {
                    columns.Add(new UserConfigurationGridColumn(field));
                }
                else
                {
                    columns.Add(new UserConfigurationGridColumnWhitchSource(field));
                }
            }

            return new UserConfigurationGridItem
            {
                Columns = columns
            };
        }

        private OrderReportDto MapResult(OrderReport item, string lang)
        {
            return new OrderReportDto
            {
                DeliveryType = item.DeliveryType != null ? new LookUpDto
                {
                    Name = item.DeliveryType.ToString().ToLowerFirstLetter().Translate(lang),
                    Value = item.DeliveryType.ToString()
                } : null,
                ClientName = string.IsNullOrEmpty(item.ClientName) ? null : new LookUpDto(item.ClientName),
                OrderAmountExcludingVAT = Math.Round(item.OrderAmountExcludingVAT.GetValueOrDefault(), 2),
                OrdersCount = item.OrdersCount,
                PalletsCount = item.PalletsCount,
                ShippingDate = item.ShippingDate?.FormatDate()
            };
        }

        private string GetFilter(ReportParametersDto filter, ref List<object> parameters)
        {
            var conditions = new List<string>();

            if (filter?.Filter == null) return "";

            if (!string.IsNullOrEmpty(filter.Filter.DeliveryType))
            {
                conditions.Add(filter.Filter.DeliveryType.ApplyEnumFilter<OrderReport, DeliveryType>(i => i.DeliveryType, ref parameters));
            }

            if (!string.IsNullOrEmpty(filter.Filter.ClientName))
            {
                conditions.Add(filter.Filter.ClientName.ApplyStringFilter<OrderReport>(i => i.ClientName, ref parameters));
            }

            if (!string.IsNullOrEmpty(filter.Filter.OrderAmountExcludingVAT))
            {
                conditions.Add($@"ROUND(SUM(""OrderAmountExcludingVAT""), 0) = ROUND({{{parameters.Count()}}}, 0)");
                parameters.Add(filter.Filter.OrderAmountExcludingVAT.ToInt());
            }

            if (!string.IsNullOrEmpty(filter.Filter.PalletsCount))
            {
                conditions.Add($@"SUM(""PalletsCount"") = {{{parameters.Count()}}}");
                parameters.Add(filter.Filter.PalletsCount.ToInt());
            }

            if (!string.IsNullOrEmpty(filter.Filter.OrdersCount))
            {
                conditions.Add($@"COUNT(""Id"") = {{{parameters.Count()}}}");
                parameters.Add(filter.Filter.OrdersCount.ToInt());
            }

            if (!string.IsNullOrEmpty(filter.Filter.ShippingDate))
            {
                conditions.Add(filter.Filter.ShippingDate.ApplyDateRangeFilter<OrderReport>(i => i.ShippingDate, ref parameters));
            }

            return conditions.Any() ? $"HAVING { string.Join(" AND ", conditions) }" : null;
        }

        private IEnumerable<string> GetGroupColumns(ReportParametersDto filter)
        {
            var columns = new List<string>();

            if (filter.DeliveryType)
            {
                columns.Add(nameof(Order.DeliveryType));
            }

            if (filter.Client)
            {
                columns.Add(nameof(Order.ClientName));
            }

            if (filter.Daily)
            {
                columns.Add(nameof(Order.ShippingDate));
            }

            return columns;
        }

        private Dictionary<string, string> GetSelectColumns(ReportParametersDto filter)
        {
            var columns = new Dictionary<string, string>
            {
                { nameof(OrderReport.DeliveryType), $@"NULL" },
                { nameof(OrderReport.ClientName), $@"NULL" },
                { nameof(OrderReport.ShippingDate), $@"NULL" },
                { nameof(OrderReport.OrdersCount), $@"COUNT(""{nameof(Order.Id)}"")" },
                { nameof(OrderReport.PalletsCount), $@"SUM(""{nameof(Order.PalletsCount)}"")" },
                { nameof(OrderReport.OrderAmountExcludingVAT), $@"SUM(""{nameof(Order.OrderAmountExcludingVAT)}"")" }
            };

            var groupColumn = GetGroupColumns(filter);

            groupColumn.ToList().ForEach(i => columns[i] = $@"""{i}""");

            return columns;
        }

        string GetWhereClause(ReportParametersDto filter)
        {
            var conditions = new List<string>();

            DateTime? startDate = filter.StartDate.ToDate();
            DateTime? endDate = filter.EndDate.ToDate();

            if (startDate.HasValue)
            {
                conditions.Add(@"""ShippingDate"" >= {0}");
            }

            if (endDate.HasValue)
            {
                conditions.Add(@"""ShippingDate"" <= {1}");
            }

            var companyId = _userProvider.GetCurrentUser()?.CompanyId;
            if (companyId != null)
            {
                conditions.Add($@"(""CompanyId"" is NULL or ""CompanyId"" = '{companyId.Value.ToString("D")}')");
            }

            return conditions.Any() ? $"WHERE { string.Join(" AND ", conditions) }" : "";
        }

        private string GetSortClause(ReportParametersDto filter)
        {
            var defaultSort = @"ORDER BY ""ShippingDate"", ""DeliveryType"", ""ClientName""";

            if (string.IsNullOrEmpty(filter?.Sort?.Name)) return defaultSort;

            var sortColumnName = filter.Sort.Name.ToUpperFirstLetter();

            var selectColumns = GetSelectColumns(filter);

            var groupColumns = GetGroupColumns(filter);

            var orderColumn = groupColumns.Contains(sortColumnName) ? $@"""{sortColumnName}""" : selectColumns[sortColumnName];

            return $@"ORDER BY {orderColumn} {(filter.Sort.Desc ? "DESC" : "ASC")}";
        }
    }
}
