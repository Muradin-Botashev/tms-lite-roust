using Domain.Enums;
using Domain.Persistables;

namespace Application.BusinessModels.Shared.Backlights
{
    public interface IBacklight<TEntity> where TEntity : IPersistable
    {
        BacklightType Type { get; }

        bool IsActive(TEntity entity);
    }
}
