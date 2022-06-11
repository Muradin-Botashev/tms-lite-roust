using Domain.Shared;
using System.IO;

namespace Domain.Services.Orders.Import
{
    public interface IOrdersImportService
    {
        Stream GenerateExcelTemplate();
        OperationDetailedResult ImportFromExcel(Stream fileStream, string fileName);
    }
}
