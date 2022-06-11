using Domain.Persistables;
using System.Collections.Generic;

namespace Domain.Shared
{
    public class EntityChanges<TEntity> where TEntity : class, IPersistable
    {
        public TEntity Entity { get; set; }
        public EntityStatus Status { get; set; }
        public List<EntityFieldChanges> FieldChanges { get; set; }
    }
}
