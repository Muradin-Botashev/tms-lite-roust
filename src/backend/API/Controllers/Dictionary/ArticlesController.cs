using API.Controllers.Shared;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Articles;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Dictionary
{
    [Route("api/articles")]
    public class ArticlesController : DictionaryController<IArticlesService, Article, ArticleDto, ArticleFilterDto> 
    {
        public ArticlesController(IArticlesService articlesService, IAppConfigurationService appConfigurationService) : base(articlesService, appConfigurationService)
        {
        }
    }
}