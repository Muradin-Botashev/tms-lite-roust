using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.DocumentTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/documentTypes")]
    public class DocumentTypesController : DictionaryController<IDocumentTypesService, DocumentType, DocumentTypeDto, DocumentTypeFilterDto>
    {
        public DocumentTypesController(IDocumentTypesService service, IAppConfigurationService appConfigurationService) : base(service, appConfigurationService) { }
    }
}