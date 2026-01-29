using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin,manager")]
public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;

    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), 201)]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] InvoiceDto dto)
    {
        var order = await _invoiceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
    [HttpPost("correction")]
    [ProducesResponseType(typeof(InvoiceDto), 201)]
    public async Task<ActionResult<InvoiceDto>> CreateCorrection([FromBody] InvoiceCorrectionDto dto)
    {
        var correction = await _invoiceService.CreateCorrectionAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = correction.Id }, correction);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<InvoiceDto>), 200)]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll()
    {
        var orders = await _invoiceService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceDto), 200)]
    public async Task<ActionResult<InvoiceDto>> GetById([FromRoute] Guid id)
    {
        var order = await _invoiceService.GetByIdAsync(id);
        return Ok(order);
    }

    [HttpPut]
    public async Task<ActionResult> Update([FromBody] InvoiceDto dto)
    {
        await _invoiceService.UpdateAsync(dto);
        return NoContent();
    }

    [HttpPut("correction")]
    public async Task<ActionResult> UpdateCorrection([FromBody] InvoiceCorrectionDto dto)
    {
        await _invoiceService.UpdateCorrectionAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        await _invoiceService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadInvoice(Guid id)
    {
        var (bytes, mimeType, originalName) = await _invoiceService.GetInvoiceFileAsync(id);
        return File(bytes, mimeType, originalName);
    }

    [HttpPost("from-order/{id}")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoiceFromOrder(Guid id)
    {
        var newInvoice = await _invoiceService.CreateFromOrderAsync(id);
        return CreatedAtAction(nameof(GetById), new { id = newInvoice.Id }, newInvoice);
    }
}
