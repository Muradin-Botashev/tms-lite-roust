using DAL;
using Microsoft.EntityFrameworkCore;

namespace Application.Shared
{
    public abstract class DbSetAndContext<TEntity> where TEntity : class
    {
        public DbSetAndContext(AppDbContext context)
        {
            Context = context;
        }

        protected AppDbContext Context { get; }
        protected DbSet<TEntity> DbSet
        {
            get
            {
                return Context.Set<TEntity>();
            }
        }
    }
}