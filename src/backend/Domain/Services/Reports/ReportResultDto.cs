using Domain.Persistables.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services.Reports
{
    public class ReportResultDto
    {
        public IEnumerable<string> Columns { get; set; }

        public IEnumerable<OrderReportDto> Data { get; set; }
    }
}
