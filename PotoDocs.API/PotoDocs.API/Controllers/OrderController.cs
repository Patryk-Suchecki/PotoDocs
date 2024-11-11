using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;
using System.Collections.Generic;

namespace PotoDocs.API.Controllers;

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
    public ActionResult<IEnumerable<OrderDto>> GetAll()
    {
        var response = _orderService.GetAll();
        return StatusCode(response.StatusCode, response);
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
        var response = await _orderService.ProcessAndCreateOrderFromPdf(file);
        return StatusCode(response.StatusCode, response);
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

    [HttpPost("{id}/cmr")]
    public async Task<ActionResult> UploadCMR(int id, [FromForm] List<IFormFile> files)
    {
        await _orderService.AddCMRFileAsync(files, id);
        return Ok();
    }
    [HttpGet("pdf/{fileName}")]
    public IActionResult GetPdf(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found");
        }

        var mimeType = "application/pdf";
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, mimeType, fileName);
    }
    [HttpDelete("pdf/{fileName}")]
    public ActionResult DeleteCMR([FromRoute] string fileName)
    {
        _orderService.DeleteCMR(fileName);
        return NoContent();
    }
}


