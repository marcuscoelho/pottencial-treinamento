using System;

namespace Pottencial.Invoices.Borders.Invoices.Exceptions
{
    public class InvalidInvoiceOperationException : Exception
    {
        public InvalidInvoiceOperationException(int number, string operation) 
            : base($"Invalid operation {operation} for invoice {number}")
        {
            Number = number;
            Operation = operation;
        }

        public int Number { get; }

        public string Operation { get; }
    }
}
