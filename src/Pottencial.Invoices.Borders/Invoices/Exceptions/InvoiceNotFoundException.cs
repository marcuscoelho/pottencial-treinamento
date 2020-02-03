using System;

namespace Pottencial.Invoices.Borders.Invoices.Exceptions
{
    public class InvoiceNotFoundException : Exception
    {
        public InvoiceNotFoundException(int number) 
            : base($"Invoice {number} not found")
        {
            this.Number = number;
        }

        public int Number { get; }
    }
}
