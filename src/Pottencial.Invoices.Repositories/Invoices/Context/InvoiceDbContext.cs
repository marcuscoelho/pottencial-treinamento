using Microsoft.EntityFrameworkCore;
using Pottencial.Invoices.Borders.Invoices.Entities;

namespace Pottencial.Invoices.Repositories.Invoices.Context
{
    public class InvoiceDbContext : DbContext
    {
        public InvoiceDbContext(DbContextOptions options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var invoice = modelBuilder.Entity<Invoice>();
            invoice.HasKey(x => x.Id);
            invoice.Property(x => x.Customer).IsRequired().HasMaxLength(100).IsUnicode(false);

            var invoiceItem = modelBuilder.Entity<InvoiceItem>();
            invoiceItem.HasKey(x => x.Id);
            invoiceItem.Property(x => x.Description).IsRequired().HasMaxLength(100).IsUnicode(false);
        }

        public DbSet<Invoice> Invoices { get; set; }
    }
}
