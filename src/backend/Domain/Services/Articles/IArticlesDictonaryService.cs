using Domain.Persistables;

namespace Domain.Services.Articles
{
    public interface IArticlesService : IDictonaryService<Article, ArticleDto, ArticleFilterDto>
    {
    }
}