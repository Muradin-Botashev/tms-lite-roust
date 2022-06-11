using Domain.Persistables;
using System.Collections.Generic;

namespace Domain.Shared
{
    public class EntityChanges
    {
        public IPersistable Entity { get; set; }

        public EntityStatus Status { get; set; }

        public List<EntityFieldChanges> FieldChanges { get; set; }

        public EntityChanges<TEntity> As<TEntity>() where TEntity : class, IPersistable
        {
            return new EntityChanges<TEntity>
            {
                Entity = (TEntity)this.Entity,
                Status = this.Status,
                FieldChanges = this.FieldChanges
            };
        }
    }
}
