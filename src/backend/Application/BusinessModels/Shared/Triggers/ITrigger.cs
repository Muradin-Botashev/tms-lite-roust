using Domain.Persistables;
using Domain.Shared;
using System.Collections.Generic;

namespace Application.BusinessModels.Shared.Triggers
{
    public interface ITrigger<TEntity> where TEntity : class, IPersistable
    {
        IEnumerable<EntityChanges<TEntity>> FilterTriggered(IEnumerable<EntityChanges<TEntity>> changes);
        void Execute(IEnumerable<EntityChanges<TEntity>> changes);
    }
}
