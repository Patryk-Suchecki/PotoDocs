using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/order")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("all")]
    public ActionResult<IEnumerable<OrderDto>> GetAll()
    {
        var orders = _orderService.GetAll();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public ActionResult<OrderDto> GetById([FromRoute] int id)
    {
        var order = _orderService.GetById(id);
        if (order == null) return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromForm] IFormFile file)
    {
        var order = await _orderService.ProcessAndCreateOrderFromPdf(file);
        if (order == null)
        {
            return BadRequest("Failed to process and create order.");
        }

        return Created($"api/order/{order.Id}", order);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute] int id)
    {
        _orderService.Delete(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    public ActionResult Update([FromBody] OrderDto dto, [FromRoute] int id)
    {
        _orderService.Update(id, dto);
        return Ok();
    }
}


