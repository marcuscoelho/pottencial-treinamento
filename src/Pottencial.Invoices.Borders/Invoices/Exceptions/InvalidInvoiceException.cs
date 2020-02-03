using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace Pottencial.Invoices.Borders.Invoices.Exceptions
{
    public class InvalidInvoiceException : Exception
    {
        public InvalidInvoiceException(IEnumerable<string> errors)
            : base("Invoice is invalid")
        {
            this.Errors = errors;
        }

        public IEnumerable<string> Errors { get; set; }
    }
}
