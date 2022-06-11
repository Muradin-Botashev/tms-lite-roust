using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.TransportCompanies;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/transportCompanies")]
    public class TransportCompaniesController : DictionaryController<ITransportCompaniesService, TransportCompany, TransportCompanyDto, TransportCompanyFilterDto> 
    {
        public TransportCompaniesController(ITransportCompaniesService transportCompaniesService, IAppConfigurationService appConfigurationService) : base(transportCompaniesService, appConfigurationService)
        {
        }
    }
}