using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Pottencial.Invoices.Borders.Invoices.Exceptions;

namespace Pottencial.Invoices.Api.Filters
{
    internal class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ILogger<HttpResponseExceptionFilter> _logger;

        public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
        {
            _logger = logger;
        }

        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is InvoiceNotFoundException notFoundException)
            {
                context.Result = new ObjectResult(notFoundException.Message)
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                };
                context.ExceptionHandled = true;
            }
            else if (context.Exception is InvalidInvoiceException invalidInvoice)
            {
                context.Result = new ObjectResult(invalidInvoice.Message)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                };
                context.ExceptionHandled = true;
            }
            else if (context.Exception is InvalidInvoiceOperationException invalidInvoiceOperation)
            {
                context.Result = new ObjectResult(invalidInvoiceOperation.Message)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                };
                context.ExceptionHandled = true;
            }
            else if (context.Exception != null)
            {
                context.Result = new ObjectResult(new { message = context.Exception.Message, details = context.Exception.StackTrace })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
