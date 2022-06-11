using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Triggers;
using AutoMapper;
using DAL.Services;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Articles;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Articles
{
    public class ArticlesService : DictoinaryServiceBase<Article, ArticleDto, ArticleFilterDto>, IArticlesService
    {
        private readonly IMapper _mapper;

        public ArticlesService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IFieldDispatcherService fieldDispatcherService,
            IEnumerable<IValidationRule<ArticleDto, Article>> validationRules,
            IChangeTrackerFactory changeTrackerFactory)
            : base(dataService, userProvider, triggersService, validationService, fieldDispatcherService, validationRules)
        {
            _mapper = ConfigureMapper().CreateMapper();
        }

        protected override IQueryable<Article> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.Company);
        }

        public override IEnumerable<LookUpDto> ForSelect()
        {
            var query = _dataService.GetDbSet<Article>().AsQueryable();
            query = ApplyRestrictions(query);

            var entities = query.OrderBy(x => x.Nart).ToList();
            foreach (var entity in entities)
            {
                yield return new LookUpDto
                {
                    Name = entity.Nart,
                    Value = entity.Id.FormatGuid()
                };
            }
        }

        public override DetailedValidationResult MapFromDtoToEntity(Article entity, ArticleDto dto)
        {
            _mapper.Map(dto, entity);
            return null;
        }

        public override ArticleDto MapFromEntityToDto(Article entity)
        {
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<ArticleDto>(entity);
        }

        protected override IQueryable<Article> ApplySort(IQueryable<Article> query, FilterFormDto<ArticleFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.Description, !string.IsNullOrEmpty(form.Sort?.Name))
                .DefaultOrderBy(i => i.Id, true);
        }

        protected override IQueryable<Article> ApplySearch(IQueryable<Article> query, FilterFormDto<ArticleFilterDto> form, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(form.Filter.Description.ApplyStringFilter<Article>(i => i.Description, ref parameters))
                         .WhereAnd(form.Filter.Nart.ApplyStringFilter<Article>(i => i.Nart, ref parameters))
                         .WhereAnd(form.Filter.TemperatureRegime.ApplyStringFilter<Article>(i => i.TemperatureRegime, ref parameters))
                         .WhereAnd(form.Filter.CompanyId.ApplyOptionsFilter<Article, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)));

            string sql = $@"SELECT * FROM ""Articles"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            if (!string.IsNullOrEmpty(form?.Filter?.Search))
            {
                var search = form.Filter.Search.ToLower();

                var isInt = int.TryParse(search, out int searchInt);

                query = query.Where(i =>
                i.TemperatureRegime.ToLower().Contains(search)
                || i.Nart.ToLower().Contains(search)
                || i.Description.ToLower().Contains(search));
            }

            return query;
        }

        protected override DetailedValidationResult ValidateDto(ArticleDto dto, Article entity, bool isConfirmed)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            DetailedValidationResult result = base.ValidateDto(dto, entity, isConfirmed);

            var currentId = dto.Id.ToGuid();
            var currentCompanyId = dto.CompanyId?.Value.ToGuid();
            var hasDuplicates = !result.IsError && _dataService.Any<Article>(x => x.Nart == dto.Nart
                                                                                    && (x.CompanyId == null || currentCompanyId == null || x.CompanyId == currentCompanyId)
                                                                                    && x.Id != currentId);

            if (hasDuplicates)
            {
                result.AddError(nameof(dto.Nart), "article.DuplicatedRecord".Translate(lang), ValidationErrorType.DuplicatedRecord);
            }

            return result;
        }

        public override Article FindByKey(ArticleDto dto)
        {
            var companyId = dto.CompanyId?.Value.ToGuid();
            return _dataService.GetDbSet<Article>()
                               .FirstOrDefault(i => i.Nart == dto.Nart && i.CompanyId == companyId);
        }

        public override IEnumerable<Article> FindByKey(IEnumerable<ArticleDto> dtos)
        {
            return dtos.Select(FindByKey).Where(x => x != null).ToList();
        }

        public override string GetEntityKey(Article entity)
        {
            return entity.Nart + "#" + (entity.CompanyId.FormatGuid() ?? string.Empty);
        }

        public override string GetDtoKey(ArticleDto dto)
        {
            return dto.Nart + "#" + (dto.CompanyId?.Value ?? string.Empty);
        }

        private MapperConfiguration ConfigureMapper()
        {
            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Article, ArticleDto>()
                    .ForMember(t => t.Id, e => e.MapFrom((s, t) => s.Id.FormatGuid()))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s, t) => s.CompanyId == null ? null : new LookUpDto(s.CompanyId.FormatGuid(), s.Company.ToString())));

                cfg.CreateMap<ArticleDto, Article>()
                    .ForMember(t => t.Id, e => e.Ignore())
                    .ForMember(t => t.CompanyId, e => e.Condition((s) => s.CompanyId != null))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s) => s.CompanyId.Value.ToGuid()));
            });
            return result;
        }

        public override ArticleDto GetDefaults()
        {
            var currentUser = _userProvider.GetCurrentUser();
            var company = currentUser?.CompanyId == null ? null : _dataService.GetById<Company>(currentUser.CompanyId.Value);

            return new ArticleDto
            {
                CompanyId = company == null ? null : new LookUpDto(company.Id.FormatGuid(), company.ToString())
            };
        }

        protected override ExcelMapper<ArticleDto> CreateExcelMapper()
        {
            return new ExcelMapper<ArticleDto>(_dataService, _userProvider, _fieldDispatcherService)
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userProvider, x => x.Name));
        }
    }
}