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
    public ActionResult<IEnumerable<OrderDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? driverEmail = null)
    {
        var response = _orderService.GetAll(page, pageSize, driverEmail);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public ActionResult<OrderDto> GetById([FromRoute] Guid id)
    {
        var response = _orderService.GetById(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromForm] IFormFile file)
    {
        var response = await _orderService.CreateFromPdf(file);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public ActionResult Update([FromBody] OrderDto dto, [FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }
        _orderService.Update(id, dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute] Guid id)
    {
        _orderService.Delete(id);
        return NoContent();
    }
}
