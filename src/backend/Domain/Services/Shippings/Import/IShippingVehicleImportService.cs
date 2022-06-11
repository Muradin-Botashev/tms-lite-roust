using Domain.Shared;
using System.IO;

namespace Domain.Services.Shippings.Import
{
    public interface IShippingVehicleImportService
    {
        Stream GenerateExcelTemplate();
        OperationDetailedResult ImportFromExcel(Stream fileStream, string fileName);
    }
}
