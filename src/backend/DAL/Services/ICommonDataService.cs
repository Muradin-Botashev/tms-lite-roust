using Domain.Persistables;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DAL.Services
{
    public interface ICommonDataService
    {
        DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class, IPersistable;

        IQueryable<TEntity> GetNoTrackingDbSet<TEntity>() where TEntity : class, IPersistable;

        IQueryable<TEntity> GetAll<TEntity>(Expression<Func<TEntity, bool>> condition) where TEntity : class, IPersistable;

        bool Any<TEntity>(Expression<Func<TEntity, bool>> condition) where TEntity : class, IPersistable;

        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;

        IQueryable<TEntity> QueryAs<TEntity>(Type entityType) where TEntity : class, IPersistable;

        TEntity GetById<TEntity>(Guid id) where TEntity : class, IPersistable;

        IEnumerable<EntityChanges<TEntity>> GetChanges<TEntity>(bool isManual = false) where TEntity : class, IPersistable;
        IEnumerable<EntityChanges> GetChanges(bool isManual = false);

        void Remove<TEntity>(TEntity entity) where TEntity : class, IPersistable;

        void IgnoreChanges<TEntity>(TEntity entity) where TEntity : class, IPersistable;

        void SaveChanges();
    }
}
