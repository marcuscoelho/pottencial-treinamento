using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pottencial.Invoices.Borders.Invoices.Entities;
using Pottencial.Invoices.Borders.Invoices.Exceptions;
using Pottencial.Invoices.Borders.Invoices.Requests;
using Pottencial.Invoices.Repositories.Invoices.Repositories;

namespace Pottencial.Invoices.UseCases.Invoices.UseCases
{
    public class CreateInvoiceUseCase
    {
        private readonly InvoiceRepository _repository;

        public CreateInvoiceUseCase(InvoiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Invoice> Invoke(CreateInvoice request)
        {
            var invoice = request.Invoice;

            var errors = new List<string>();

            if (invoice.Date != DateTime.Today)
            {
                errors.Add("Invoice date must be today");
            }

            if (invoice.Number <= 0)
            {
                errors.Add("Invoice number must be greater than 0");
            }

            if (invoice.Amount < 0)
            {
                errors.Add("Invoice amount must be greater than or equal to 0");
            }
            else if (invoice.Amount != (invoice.Items ?? Enumerable.Empty<InvoiceItem>()).Sum(x => x.Amount))
            {
                errors.Add("Invoice amount must be equal to the sum of items amount");
            }

            if (string.IsNullOrWhiteSpace(invoice.Customer))
            {
                errors.Add("Invoice customer must not be null or empty");
            }

            if (invoice.Status != InvoiceStatus.Created)
            {
                errors.Add("Invoice status must be created");
            }

            if (invoice.Items == null || invoice.Items.Count == 0)
            {
                errors.Add("Invoice status must have at least one item");
            }

            if (invoice.Items != null)
            {
                int a = 1;
                foreach (var x in invoice.Items.OrderBy(x => x.Number))
                {
                    if (x.Number != a)
                    {
                        errors.Add($"Invoice item with number {a} not found");

                        break;
                    }

                    a++;
                }

                for (int index = 0; index < invoice.Items.Count; index++)
                {
                    var item = invoice.Items[index];

                    if (item.Number <= 0)
                    {
                        errors.Add($"Item number at position {index} must be greater than 0");
                    }

                    if (string.IsNullOrWhiteSpace(item.Description))
                    {
                        errors.Add($"Item description at position {index} must not be null or empty");
                    }

                    if (item.Quantity <= 0)
                    {
                        errors.Add($"Item quantity at position {index} must be greater than 0");
                    }

                    if (item.UnitPrice < 0)
                    {
                        errors.Add($"Item unit price at position {index} must be greater than or equal to 0");
                    }

                    if (item.Amount < 0)
                    {
                        errors.Add($"Item amount at position {index} must be greater than or equal to 0");
                    }
                    else if (item.Amount != item.Quantity * item.UnitPrice)
                    {
                        errors.Add($"Item amount at position {index} must be equal to quantity x unit price");
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidInvoiceException(errors);
            }

            var existing = await _repository.GetByNumber(invoice.Number);

            if (existing != null)
            {
                throw new DuplicatedInvoiceException(existing.Number);
            }

            await _repository.Insert(invoice);

            return invoice;
        }
    }
}
