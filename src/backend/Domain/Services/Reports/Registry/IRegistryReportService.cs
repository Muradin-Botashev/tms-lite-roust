using System.IO;

namespace Domain.Services.Reports.Registry
{
    public interface IRegistryReportService
    {
        Stream ExportReport(RegistryReportParameters filter);
    }
}
