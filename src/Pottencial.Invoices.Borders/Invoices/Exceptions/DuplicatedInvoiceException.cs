using System;

namespace Pottencial.Invoices.Borders.Invoices.Exceptions
{
    public class DuplicatedInvoiceException : Exception
    {
        public DuplicatedInvoiceException(int number)
            : base($"Invoice number {number} already exists")
        {
            Number = number;
        }

        public int Number { get; }
    }
}
