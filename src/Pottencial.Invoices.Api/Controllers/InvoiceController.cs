using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pottencial.Invoices.Borders.Invoices.Entities;
using Pottencial.Invoices.Borders.Invoices.Exceptions;
using Pottencial.Invoices.Borders.Invoices.Requests;
using Pottencial.Invoices.UseCases.Invoices.UseCases;

namespace Pottencial.Invoices.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceUseCases _useCases;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            InvoiceUseCases useCases,
            ILogger<InvoiceController> logger)
        {
            _useCases = useCases;
            _logger = logger;
        }

        /// <summary>
        /// Recupera a lista de invoices cadastrados, dado os filtros informados.
        /// Invoices com o status Cancelled não são retornados.
        /// </summary>
        /// <param name="customer">Nome do cliente.</param>
        /// <param name="skip">Quantidade de registros para saltar.</param>
        /// <param name="take">Quantidade de registros a serem retornados. Padrão é 10.</param>
        /// <returns>Lista de invoices cadastrados conforme os filtros informados.</returns>
        [HttpGet]
        public async Task<ActionResult<Invoice[]>> Get(string customer = null, int skip = 0, int take = 10)
        {
            var request = new GetInvoices { Customer = customer, Skip = skip, Take = take };

            var invoices = await _useCases.Invoke<Invoice[]>(request);

            return Ok(invoices);
        }

        /// <summary>
        /// Recupa uma invoice a partir do número.
        /// </summary>
        /// <param name="number">Número da invoice.</param>
        /// <returns>Invoice que possui o número informado.</returns>
        [HttpGet("{number}")]
        public async Task<ActionResult<Invoice>> Get([FromRoute]int number)
        {
            var request = new GetInvoiceByNumber { Number = number };

            var invoice = await _useCases.Invoke<Invoice>(request);

            return Ok(invoice);
        }

        /// <summary>
        /// Cria uma nova invoice.
        /// </summary>
        /// <param name="request">Dados da requisição.</param>
        /// <returns>Invoice criada.</returns>
        [HttpPost]
        public async Task<ActionResult<Invoice>> Post([FromBody]CreateInvoice request)
        {
            var invoice = await _useCases.Invoke<Invoice>(request);

            return Ok(invoice);
        }

        /// <summary>
        /// Atualiza os dados de uma invoice.
        /// </summary>
        /// <param name="request">Dados da requisição.</param>
        [HttpPut]
        public async Task<ActionResult> Put([FromBody]ChangeInvoice request)
        {
            await _useCases.Invoke(request);

            return Ok();
        }

        /// <summary>
        /// Faz a submissão da invoice informada.
        /// </summary>
        /// <param name="request">Dados da requisição.</param>
        [HttpPost("submit")]
        public async Task<ActionResult> Submit([FromBody]SubmitInvoice request)
        {
            await _useCases.Invoke(request);

            return Ok();
        }

        /// <summary>
        /// Cancela uma invoice.
        /// </summary>
        /// <param name="number">Número da invoice para cancelar.</param>
        [HttpDelete("cancel/{number}")]
        public async Task<ActionResult> Delete([FromRoute]int number)
        {
            var request = new CancelInvoice { Number = number };

            await _useCases.Invoke(request);

            return Ok();
        }
    }
}
