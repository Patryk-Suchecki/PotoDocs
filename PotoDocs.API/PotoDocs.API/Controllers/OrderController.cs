using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

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
        var order = await _orderService.ProcessAndCreateOrderFromPdf(file);
        if (order == null)
        {
            return BadRequest("Nie udało się przetworzyć i stworzyć zamówienia.");
        }

        return CreatedAtAction(nameof(GetById), new { id = order.Data.InvoiceNumber }, order);
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
    [Authorize]
    public async Task<ActionResult> UploadCMR(int id, [FromForm] List<IFormFile> cmrFiles)
    {
        var order = _orderService.GetById(id);
        if (order == null)
        {
            return NotFound($"Order with ID {id} not found.");
        }

        if (cmrFiles == null || cmrFiles.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var cmrFileUrls = new List<string>();
        foreach (var file in cmrFiles)
        {
            // Zapis pliku do folderu wwwroot/pdfs
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdfs", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Dodanie do listy URLi
            var relativePath = Path.Combine("/pdfs", fileName); // Ścieżka względna do pliku
            cmrFileUrls.Add(relativePath);

            // Zapis danych pliku CMR w bazie danych
            var cmrFile = new CMRFile
            {
                Url = relativePath,
                OrderId = id
            };
            _orderService.AddCMRFile(cmrFile); // Dodanie pliku CMR do zamówienia
        }

        return Ok(new { cmrFileUrls });
    }
}


