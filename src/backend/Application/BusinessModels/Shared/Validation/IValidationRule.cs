using Domain.Persistables;
using Domain.Services;
using Domain.Shared;

namespace Application.BusinessModels.Shared.Validation
{
    public interface IValidationRule<TDto, TEntity>
        where TDto : IDto 
        where TEntity : IPersistable
    {
        DetailedValidationResult Validate(TDto dto, TEntity entity);
        bool IsApplicable(string fieldName);
    }
}
