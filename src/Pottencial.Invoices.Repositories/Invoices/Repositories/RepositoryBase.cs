using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Pottencial.Invoices.Repositories.Invoices.Repositories
{
    public abstract class RepositoryBase<T>
        where T : class
    {
        protected DbContext Context { get; }

        public RepositoryBase(DbContext context)
        {
            Context = context;
        }

        public virtual async Task Insert(T entity)
        {
            Context.Add(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task Update(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync();
        }

        public virtual async Task Delete(T entity)
        {
            Context.Entry(entity).State = EntityState.Deleted;
            await Context.SaveChangesAsync();
        }
    }
}
