using System.Threading.Tasks;
using Pottencial.Invoices.Borders.Invoices.Entities;
using Pottencial.Invoices.Borders.Invoices.Exceptions;
using Pottencial.Invoices.Borders.Invoices.Requests;
using Pottencial.Invoices.Repositories.Invoices.Repositories;

namespace Pottencial.Invoices.UseCases.Invoices.UseCases
{
    public class GetInvoiceByNumberUseCase
    {
        private readonly InvoiceRepository _repository;

        public GetInvoiceByNumberUseCase(InvoiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Invoice> Invoke(GetInvoiceByNumber request)
        {
            var invoice = await _repository.GetByNumber(request.Number);

            if (invoice == null)
            {
                throw new InvoiceNotFoundException(request.Number);
            }

            return invoice;
        }
    }
}
