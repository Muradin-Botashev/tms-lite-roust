using Domain.Shared;

namespace Domain.Services.Import
{
    public interface IOpenImportService
    {
        void ImportShippings(string requestData);
    }
}
