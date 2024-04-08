using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UTask.Data.Services;

namespace UTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly UTaskService UTaskService;

        public InvoicesController(UTaskService uTaskService)
        {
            UTaskService = uTaskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices([FromHeader (Name = "Authorization")] string token)
        {
            var invoices = await UTaskService.GetInvoicesAsync(token);
            return Ok(invoices);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice([FromHeader(Name = "Authorization")] string token, int id)
        {
            var result = await UTaskService.DeleteInvoiceAsync(token, id);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice([FromHeader(Name = "Authorization")] string token, int id, string status)
        {
            var result = await UTaskService.UpdateInvoiceAsync(token, id, status);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById([FromHeader(Name = "Authorization")] string token, int invoiceId)
        {
            var invoice = await UTaskService.GetInvoiceByIdAsync(token, invoiceId);
            if (invoice != null)
            {
                return Ok(invoice);
            }
            return BadRequest();
        }

    }
}
