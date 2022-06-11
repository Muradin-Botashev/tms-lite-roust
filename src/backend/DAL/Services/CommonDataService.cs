using Domain.Persistables;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DAL.Services
{
    public class CommonDataService : ICommonDataService
    {
        private readonly AppDbContext _context;

        public CommonDataService(AppDbContext context)
        {
            _context = context;
        }

        public TEntity GetById<TEntity>(Guid id) where TEntity : class, IPersistable
        {
            return GetDbSet<TEntity>().Find(id);
        }

        public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class, IPersistable
        {
            return _context.Set<TEntity>();
        }

        public IQueryable<TEntity> GetNoTrackingDbSet<TEntity>() where TEntity : class, IPersistable
        {
            return GetDbSet<TEntity>().AsNoTracking();
        }

        public IQueryable<TEntity> GetAll<TEntity>(Expression<Func<TEntity, bool>> condition) where TEntity : class, IPersistable
        {
            return _context.Set<TEntity>().Where(condition);
        }

        public bool Any<TEntity>(Expression<Func<TEntity, bool>> condition) where TEntity : class, IPersistable
        {
            return _context.Set<TEntity>().Local.Any(condition.Compile()) || _context.Set<TEntity>().Any(condition);
        }

        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class
        {
            return _context.Query<TEntity>().FromSql(sql, parameters);
        }

        public IEnumerable<EntityChanges<TEntity>> GetChanges<TEntity>(bool isManual = false) 
            where TEntity : class, IPersistable
        {
            var entries = _context.ChangeTracker.Entries<TEntity>().ToList();
            foreach (var entry in entries)
            {
                var fieldChanges = GetFieldChanges(entry, isManual);

                yield return new EntityChanges<TEntity>
                {
                    Entity = entry.Entity,
                    FieldChanges = fieldChanges
                };
            }
        }

        public IEnumerable<EntityChanges> GetChanges(bool isManual = false)
        {
            var entries = _context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                var fieldChanges = GetFieldChanges(entry, isManual);
                yield return new EntityChanges
                {
                    Entity = (IPersistable)entry.Entity,
                    Status = ConvertEntityStatus(entry.State),
                    FieldChanges = fieldChanges
                };
            }
        }

        private EntityStatus ConvertEntityStatus(EntityState state)
        {
            switch(state)
            {
                case EntityState.Added: return EntityStatus.Added;
                case EntityState.Deleted: return EntityStatus.Deleted;
                case EntityState.Modified: return EntityStatus.Updated;
                default: return default;
            }
        }

        private List<EntityFieldChanges> GetFieldChanges(EntityEntry entity, bool isManual)
        {
            var fieldChanges = new List<EntityFieldChanges>();
            var fields = entity.Properties.Where(x => x.IsModified || (entity.State == EntityState.Added && x.CurrentValue != default))
                                          .ToList();
            foreach (var field in fields)
            {
                var fieldChange = new EntityFieldChanges
                {
                    FieldName = field.Metadata.Name,
                    OldValue = field.OriginalValue,
                    NewValue = field.CurrentValue,
                    IsManual = isManual
                };
                fieldChanges.Add(fieldChange);
            }
            return fieldChanges;
        }

        public virtual void SaveChanges()
        {
            _context.SaveChanges();
        }

        void ICommonDataService.Remove<TEntity>(TEntity entity)
        {
            this.GetDbSet<TEntity>().Remove(entity);
        }

        public void IgnoreChanges<TEntity>(TEntity entity) where TEntity: class, IPersistable
        {
            this._context.Entry(entity).State = EntityState.Unchanged;
        }

        IQueryable<TEntity> ICommonDataService.QueryAs<TEntity>(Type entityType)
        {
            var getMethod = typeof(CommonDataService).GetMethod(nameof(GetDbSet)).MakeGenericMethod(entityType);
            var refEntity = getMethod.Invoke(this, null);

            return refEntity as IQueryable<TEntity>;
        }
    }
}
