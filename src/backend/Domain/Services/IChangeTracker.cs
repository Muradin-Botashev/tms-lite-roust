using Domain.Persistables;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Domain.Services
{
    public interface IChangeTracker
    {
        IChangeTracker Add<TEntity>(Expression<Func<TEntity, object>> property, string formatString = null);

        IChangeTracker Remove<TEntity>(Expression<Func<TEntity, object>> property);

        IChangeTracker TrackAll<TEntity>();

        void LogTrackedChanges<TEntity>(EntityChanges<TEntity> change) where TEntity : class, IPersistable;
        void LogTrackedChanges<TEntity>(IEnumerable<EntityChanges<TEntity>> changes) where TEntity : class, IPersistable;

        void LogTrackedChanges(EntityChanges change);

        void LogTrackedChanges(IEnumerable<EntityChanges> changes);

        void LogTrackedChanges();
    }
}
