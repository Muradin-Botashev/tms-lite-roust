using Domain.Persistables;
using Domain.Shared.FormFilters;
using System.Collections.Generic;

namespace Domain.Services.Translations
{
    public interface ITranslationsService : IDictonaryService<Translation, TranslationDto, SearchFilterDto>
    {
        IEnumerable<TranslationDto> GetAll();

        Translation FindByKey(string name);
    }
}