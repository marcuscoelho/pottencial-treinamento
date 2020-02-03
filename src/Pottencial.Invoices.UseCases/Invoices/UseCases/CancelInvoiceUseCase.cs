using System.Threading.Tasks;
using Pottencial.Invoices.Borders.Invoices.Entities;
using Pottencial.Invoices.Borders.Invoices.Exceptions;
using Pottencial.Invoices.Borders.Invoices.Requests;
using Pottencial.Invoices.Repositories.Invoices.Repositories;

namespace Pottencial.Invoices.UseCases.Invoices.UseCases
{
    public class CancelInvoiceUseCase
    {
        private readonly InvoiceRepository _repository;

        public CancelInvoiceUseCase(InvoiceRepository repository)
        {
            _repository = repository;
        }

        public async Task Invoke(CancelInvoice request)
        {
            var invoice = await _repository.GetByNumber(request.Number);

            if (invoice == null)
            {
                throw new InvoiceNotFoundException(request.Number);
            }

            if (invoice.Status == Status.Cancelled)
            {
                throw new InvalidInvoiceOperationException(request.Number, "Cancel");
            }

            invoice.Status = Status.Cancelled;

            await _repository.Update(invoice);
        }
    }
}
