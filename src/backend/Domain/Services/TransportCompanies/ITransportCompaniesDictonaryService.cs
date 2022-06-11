using Domain.Persistables;

namespace Domain.Services.TransportCompanies
{
    public interface ITransportCompaniesService : IDictonaryService<TransportCompany, TransportCompanyDto, TransportCompanyFilterDto>
    {
    }
}