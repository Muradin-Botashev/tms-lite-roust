using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Companies;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/companies")]
    public class CompaniesController : DictionaryController<ICompaniesService, Company, CompanyDto, CompanyFilterDto>
    {
        public CompaniesController(ICompaniesService service, IAppConfigurationService appConfigurationService) : base(service, appConfigurationService) { }

    }
}
