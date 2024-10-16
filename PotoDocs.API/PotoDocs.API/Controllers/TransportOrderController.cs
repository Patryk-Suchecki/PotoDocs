using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using PotoDocs.API.Models;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/transportorder")]
[ApiController]
[Authorize]
public class TransportOrderController : ControllerBase
{
    //b
    private readonly ITransportOrderService _transportOrderService;

    public TransportOrderController(ITransportOrderService transportOrderService)
    {
        _transportOrderService = transportOrderService;
    }

    [HttpGet("all")]
    public ActionResult<IEnumerable<TransportOrderDto>> GetAll()
    {
        var orders = _transportOrderService.GetAll();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public ActionResult<TransportOrderDto> GetById([FromRoute] int id)
    {
        var order = _transportOrderService.GetById(id);
        if (order == null) return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<TransportOrderDto>> Create([FromForm] IFormFile file)
    {
        var transportOrder = await _transportOrderService.ProcessAndCreateOrderFromPdf(file);
        if (transportOrder == null)
        {
            return BadRequest("Failed to process and create order.");
        }

        return Created($"api/transportorder/{transportOrder.Id}", transportOrder);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute] int id)
    {
        _transportOrderService.Delete(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    public ActionResult Update([FromBody] TransportOrderDto dto, [FromRoute] int id)
    {
        _transportOrderService.Update(id, dto);
        return Ok();
    }
}


