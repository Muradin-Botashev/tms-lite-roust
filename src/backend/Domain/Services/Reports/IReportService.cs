using Domain.Services.AppConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Domain.Services.Reports
{
    public interface IReportService
    {
        ReportResultDto GetReport(ReportParametersDto filter);

        Stream ExportReport(ReportParametersDto filter);

        UserConfigurationGridItem GetReportConfiguration();
    }
}
