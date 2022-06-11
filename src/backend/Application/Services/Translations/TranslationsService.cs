using Application.BusinessModels.Shared.Validation;
using Application.Shared;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.FormFilters;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Translations
{
    public class TranslationsService : DictoinaryServiceBase<Translation, TranslationDto, SearchFilterDto>, ITranslationsService
    {
        public TranslationsService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<TranslationDto, Translation>> validationRules) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules) 
        { }

        public IEnumerable<TranslationDto> GetAll()
        {
            return _dataService.GetDbSet<Translation>().ToList().Select(x=>
            {
                return new TranslationDto
                {
                    Id = x.Id.FormatGuid(),
                    Name = x.Name,
                    Ru = x.Ru,
                    En = x.En
                };
            } );
        }

        public Translation FindByKey(string name)
        {
            return _dataService.GetDbSet<Translation>().Where(x => x.Name == name).FirstOrDefault();
        }

        public override DetailedValidationResult MapFromDtoToEntity(Translation entity, TranslationDto dto)
        {
            if(!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);
            entity.Name = dto.Name;
            entity.En = dto.En;
            entity.Ru = dto.Ru;

            return new DetailedValidationResult(entity.Id);
        }

        public override TranslationDto MapFromEntityToDto(Translation entity)
        {
            return new TranslationDto
            {
                Id = entity.Id.FormatGuid(),
                Name = entity.Name,
                En = entity.En,
                Ru = entity.Ru,
            };
        }

        protected override IQueryable<Translation> ApplySearch(IQueryable<Translation> query, FilterFormDto<SearchFilterDto> form, List<string> columns = null)
        {
            return query;
        }
    }
}