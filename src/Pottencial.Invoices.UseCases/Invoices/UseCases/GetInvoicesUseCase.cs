using System.Threading.Tasks;
using Pottencial.Invoices.Borders.Invoices.Entities;
using Pottencial.Invoices.Borders.Invoices.Requests;
using Pottencial.Invoices.Repositories.Invoices.Repositories;

namespace Pottencial.Invoices.UseCases.Invoices.UseCases
{
    public class GetInvoicesUseCase
    {
        private readonly InvoiceRepository _repository;

        public GetInvoicesUseCase(InvoiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Invoice[]> Invoke(GetInvoices request)
        {
            return await _repository.GetInvoices(request.Customer, request.Skip, request.Take);
        }
    }
}
