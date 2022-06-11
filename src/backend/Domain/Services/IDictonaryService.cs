using System;
using System.Collections.Generic;
using System.IO;
using Domain.Services.AppConfiguration;
using Domain.Shared;

namespace Domain.Services
{
    public interface IDictonaryService<TEntity, TDto, TFilter> : IService
    {
        SearchResult<TDto> Search(FilterFormDto<TFilter> form);

        IEnumerable<LookUpDto> ForSelect();
        IEnumerable<LookUpDto> ForSelect(Guid? companyId);

        IEnumerable<LookUpDto> ForSelect(string field, FilterFormDto<TFilter> form);

        AppResult Import(IEnumerable<TDto> entityFrom, bool isConfirmed);

        AppResult ImportFromExcel(Stream fileStream, bool isConfirmed);

        Stream ExportToExcel(FilterFormDto<TFilter> form);

        DetailedValidationResult SaveOrCreate(TDto entityFrom, bool isConfirmed);
        TDto Get(Guid id);

        ValidateResult Delete(Guid id);

        TDto GetDefaults();

        UserConfigurationDictionaryItem GetFormConfiguration(Guid id, UserConfigurationDictionaryItem defaultConfig);
    }
}