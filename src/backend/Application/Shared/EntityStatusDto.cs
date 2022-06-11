using Domain.Persistables;

namespace Application.Shared
{
    public class EntityStatusDto<TEntity> where TEntity: class, IPersistable
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public TEntity Entity { get; set; }
    }
}
