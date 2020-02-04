using System;
using System.Linq;
using Bogus;
using Pottencial.Invoices.Borders.Invoices.Entities;

namespace Pottencial.Invoices.Api.Tests.Fakers
{
    public static class InvoiceFaker
    {
        public static Invoice Create(int number, string customer = null, DateTime? date = null, InvoiceStatus? status = null, int? items = null)
        {
            var itemFaker = new Faker<InvoiceItem>()
                .RuleFor(item => item.Description, faker => faker.Lorem.Sentence())
                .RuleFor(item => item.Quantity, faker => faker.Random.Decimal(1, 10))
                .RuleFor(item => item.UnitPrice, faker => faker.Random.Decimal(1, 100));

            return new Faker<Invoice>()
                .RuleFor(invoice => invoice.Number, faker => number)
                .RuleFor(invoice => invoice.Customer, faker => customer ?? faker.Name.FullName())
                .RuleFor(invoice => invoice.Date, faker => date ?? DateTime.Today)
                .RuleFor(invoice => invoice.Status, faker => status ?? InvoiceStatus.Created)
                .RuleFor(invoice => invoice.Items, faker => itemFaker.Generate(faker.Random.Int(1, items ?? 5)))
                .FinishWith((faker, invoice) =>
                {
                    var number = 1;
                    foreach (var item in invoice.Items)
                    {
                        item.Number = number++;
                        item.Amount = item.Quantity * item.UnitPrice;
                        invoice.Amount += item.Amount;
                    }
                })
                .Generate();
        }
    }
}
