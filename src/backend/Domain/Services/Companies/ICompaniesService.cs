using Domain.Persistables;

namespace Domain.Services.Companies
{
    public interface ICompaniesService : IDictonaryService<Company, CompanyDto, CompanyFilterDto>
    {
    }
}
