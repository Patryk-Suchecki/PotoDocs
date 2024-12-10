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
    public ActionResult<PaginatedResponse<OrderDto>> GetAll([FromQuery] string? filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var response = _orderService.GetAll(filter, page, pageSize);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public ActionResult<OrderDto> GetById([FromRoute] int id)
    {
        var response = _orderService.GetById(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromForm] IFormFile file)
    {
        var response = await _orderService.ProcessAndCreateOrderFromPdf(file);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public ActionResult Update([FromBody] OrderDto dto, [FromRoute] int id)
    {
        _orderService.Update(id, dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute] int id)
    {
        _orderService.Delete(id);
        return NoContent();
    }
}
