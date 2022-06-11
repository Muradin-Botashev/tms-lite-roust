using Domain.Shared;
using System.IO;

namespace Domain.Services.Warehouses.Import
{
    public interface IPoolingWarehousesImportService
    {
        Stream GenerateExcelTemplate();
        OperationDetailedResult ImportFromExcel(Stream fileStream, string fileName);
    }
}
