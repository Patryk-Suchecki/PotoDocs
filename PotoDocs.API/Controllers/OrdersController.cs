using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;
using System.Text.Json;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), 201)]
    public async Task<ActionResult<OrderDto>> Create([FromForm] string orderDtoJson, [FromForm] IEnumerable<IFormFile> orderFiles, [FromForm] IEnumerable<IFormFile> cmrFiles)
    {

        var dto = JsonSerializer.Deserialize<OrderDto>(orderDtoJson, _jsonOptions);

        if (dto == null)
        {
            return BadRequest("Nie można zdeserializować danych zlecenia.");
        }

        var order = await _orderService.CreateAsync(dto, orderFiles, cmrFiles);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    public async Task<ActionResult<OrderDto>> GetById([FromRoute] Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, [FromForm] string orderDtoJson, [FromForm] IEnumerable<IFormFile> orderFiles, [FromForm] IEnumerable<IFormFile> cmrFiles, [FromForm] IEnumerable<Guid> fileIdsToDelete)
    {
        var dto = JsonSerializer.Deserialize<OrderDto>(orderDtoJson, _jsonOptions);

        if (dto == null)
        {
            return BadRequest("Nie można zdeserializować danych zlecenia.");
        }

        await _orderService.UpdateAsync(id, dto, orderFiles, cmrFiles, fileIdsToDelete);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        await _orderService.DeleteAsync(id);
        return NoContent();
    }

    [ProducesResponseType(typeof(OrderDto), 200)]
    [HttpPost("parse")]
    public async Task<ActionResult<OrderDto>> ParseOrder([FromForm] IFormFile file)
    {
        var order = await _orderService.ParseOrderAsync(file);
        return Ok(order);
    }
    [ProducesResponseType(typeof(OrderDto), 200)]
    [HttpPost("parse/{fileId}")]
    public async Task<ActionResult<OrderDto>> ParseExistingOrder(Guid fileId)
    {
        var order = await _orderService.ParseExistingOrderAsync(fileId);
        return Ok(order);
    }

    [HttpGet("files/{fileId}")]
    public async Task<IActionResult> DownloadFile(Guid fileId)
    {
        var (bytes, mimeType, originalName) = await _orderService.GetFileAsync(fileId);
        return File(bytes, mimeType, originalName);
    }

    [HttpDelete("files/{fileId}")]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        await _orderService.DeleteFileAsync(fileId);
        return NoContent();
    }

    [HttpPost("{id}/send-documents")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendDocuments(Guid id)
    {
        await _orderService.SendDocumentsAsync(id);
        return Ok();
    }
    [HttpPost("mark-as-sent")]
    public async Task<IActionResult> MarkDocumentsAsSent([FromBody] List<Guid> orderIds, [FromQuery] DateTime sentDate)
    {
        await _orderService.MarkOrdersAsSentAsync(orderIds, sentDate);
        return Ok();
    }
}
