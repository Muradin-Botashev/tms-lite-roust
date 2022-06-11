using Application.BusinessModels.Shared.Validation;
using Application.Shared;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.Injections;
using Domain.Shared;
using Domain.Shared.FormFilters;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Injections
{
    public class InjectionsService : DictoinaryServiceBase<Injection, InjectionDto, SearchFilterDto>, IInjectionsService
    {
        public InjectionsService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<InjectionDto, Injection>> validationRules) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules) 
        { }

        public override DetailedValidationResult MapFromDtoToEntity(Injection entity, InjectionDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);
            entity.Type = dto.Type;
            entity.FileName = dto.FileName;
            entity.Status = dto.Status;
            entity.ProcessTimeUtc = dto.ProcessTimeUtc;

            return new DetailedValidationResult(entity.Id);
        }

        public override InjectionDto MapFromEntityToDto(Injection entity)
        {
            return new InjectionDto
            {
                Id = entity.Id.FormatGuid(),
                Type = entity.Type,
                FileName = entity.FileName,
                Status = entity.Status,
                ProcessTimeUtc = entity.ProcessTimeUtc
            };
        }

        public IEnumerable<InjectionDto> GetByTaskName(string taskName)
        {
            var resultEntries = _dataService.GetDbSet<Injection>().Where(i => i.Type == taskName);
            var resultDtos = resultEntries.Select(MapFromEntityToDto).ToArray();
            return resultDtos;
        }

        protected override IQueryable<Injection> ApplySearch(IQueryable<Injection> query, FilterFormDto<SearchFilterDto> form, List<string> columns = null)
        {
            return query;
        }
    }
}
