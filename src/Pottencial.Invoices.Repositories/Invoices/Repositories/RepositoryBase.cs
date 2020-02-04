using System;
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

        public async Task Insert(T entity)
        {
            BeforeInsert(entity);
            
            Context.Add(entity);
            await Context.SaveChangesAsync();

            AfterInsert(entity);
        }

        protected virtual void AfterInsert(T entity)
        {
        }

        protected virtual void BeforeInsert(T entity)
        {
        }

        public async Task Update(T entity)
        {
            BeforeUpdate(entity);

            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            AfterUpdate(entity);
        }

        protected virtual void BeforeUpdate(T entity)
        {
        }

        protected virtual void AfterUpdate(T entity)
        {
        }

        public virtual async Task Delete(T entity)
        {
            Context.Entry(entity).State = EntityState.Deleted;
            await Context.SaveChangesAsync();
        }
    }
}
