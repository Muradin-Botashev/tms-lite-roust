using Application.BusinessModels.Shared.Validation;
using Application.Shared;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.TaskProperties;
using Domain.Shared;
using Domain.Shared.FormFilters;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.TaskProperties
{
    public class TaskPropertiesService : DictoinaryServiceBase<TaskProperty, TaskPropertyDto, SearchFilterDto>, ITaskPropertiesService
    {
        public TaskPropertiesService(
            ICommonDataService dataService, 
            IUserProvider userProvider, 
            ITriggersService triggersService, 
            IValidationService validationService, 
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<TaskPropertyDto, TaskProperty>> validationRules) 
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules) 
        { }

        public override DetailedValidationResult MapFromDtoToEntity(TaskProperty entity, TaskPropertyDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = Guid.Parse(dto.Id);
            entity.TaskName = dto.TaskName;
            entity.Properties = dto.Properties;

            return new DetailedValidationResult(entity.Id);
        }

        public override TaskPropertyDto MapFromEntityToDto(TaskProperty entity)
        {
            return new TaskPropertyDto
            {
                Id = entity.Id.FormatGuid(),
                TaskName = entity.TaskName,
                Properties = entity.Properties
            };
        }

        public IEnumerable<TaskPropertyDto> GetByTaskName(string taskName)
        {
            var entries = _dataService.GetDbSet<TaskProperty>().Where(p => p.TaskName == taskName).ToList();
            var result = entries.Select(MapFromEntityToDto).ToList();
            return result;
        }

        protected override IQueryable<TaskProperty> ApplySearch(IQueryable<TaskProperty> query, FilterFormDto<SearchFilterDto> form, List<string> columns = null)
        {
            return query;
        }
    }
}
