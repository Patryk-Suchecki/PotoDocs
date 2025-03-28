using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin,manager")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    public ActionResult<IEnumerable<OrderDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? driverEmail = null)
    {
        var orders = _orderService.GetAll(page, pageSize, driverEmail);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    public ActionResult<OrderDto> GetById([FromRoute] Guid id)
    {
        var order = _orderService.GetById(id);
        return Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), 201)]
    public async Task<ActionResult<OrderDto>> Create([FromForm] IFormFile file)
    {
        var createdOrder = await _orderService.CreateFromPdf(file);
        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromBody] OrderDto dto, [FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _orderService.Update(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        _orderService.Delete(id);
        return NoContent();
    }
}
