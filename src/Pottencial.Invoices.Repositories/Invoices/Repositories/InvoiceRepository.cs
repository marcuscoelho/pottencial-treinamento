using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pottencial.Invoices.Borders.Invoices.Entities;
using Pottencial.Invoices.Repositories.Invoices.Context;

namespace Pottencial.Invoices.Repositories.Invoices.Repositories
{
    public class InvoiceRepository : RepositoryBase<Invoice>
    {
        public InvoiceRepository(InvoiceDbContext context)
            : base(context)
        {
        }

        public async Task<Invoice[]> GetInvoices(string customer, int skip, int take)
        {
            IQueryable<Invoice> invoices = Context.Set<Invoice>()
                .Include(x => x.Items)
                .Where(x => x.Status != Status.Cancelled);

            if (!string.IsNullOrWhiteSpace(customer))
            {
                invoices = invoices.Where(x => x.Customer.StartsWith(customer));
            }

            return await invoices
                .OrderByDescending(x => x.Date)
                .Skip(skip)
                .Take(Math.Min(take, 50))
                .ToArrayAsync();
        }

        public async Task<Invoice> GetByNumber(int number)
        {
            return await Context.Set<Invoice>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Number == number);
        }

        public override Task Delete(Invoice entity)
        {
            throw new InvalidOperationException("Cannot delete a invoice. Cancel it instead.");
        }
    }
}
