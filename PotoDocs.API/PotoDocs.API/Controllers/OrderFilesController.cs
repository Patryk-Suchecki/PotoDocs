using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

[Route("api/orders/")]
[ApiController]
[Authorize(Roles = "admin,manager")]
public class OrderFilesController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderFilesController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("{invoiceNumber}/cmr")]
    public async Task<ActionResult> UploadCMR(int invoiceNumber, [FromForm] List<IFormFile> files)
    {
        var response = await _orderService.AddCMRFileAsync(files, invoiceNumber);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{invoiceNumber}/cmr/{fileName}")]
    public ActionResult DeleteCMR(string fileName)
    {
        _orderService.DeleteCMR(fileName);
        return NoContent();
    }

    [HttpGet("{invoiceNumber}/pdf/{fileName}")]
    public IActionResult GetPdf(string fileName)
    {
        if (fileName.Contains("..") || Path.GetInvalidFileNameChars().Any(fileName.Contains))
        {
            return BadRequest("Nazwa pliku jest nieprawidłowa.");
        }
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Plik nie został znaleziony.");
        }

        var mimeType = "application/pdf";
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, mimeType, fileName);
    }

    [HttpGet("{invoiceNumber}/invoice")]
    public async Task<IActionResult> GetInvoiceAsync(int invoiceNumber)
    {
        var pdfData = await _orderService.CreateInvoicePDF(invoiceNumber);

        if (pdfData == null || pdfData.Length == 0)
        {
            return BadRequest("Nie udało się wygenerować faktury.");
        }

        return File(pdfData, "application/pdf");
    }

    [HttpGet("invoices/{year}/{month}")]
    public async Task<IActionResult> GetInvoices(int year, int month)
    {
        var pdfData = await _orderService.CreateInvoices(year, month);

        if (pdfData == null || pdfData.Length == 0)
        {
            return BadRequest("Nie udało się wygenerować faktury.");
        }

        return File(pdfData, "application/zip");
    }
    [HttpGet("invoices")]
    public async Task<IActionResult> GetAvailableYearsAndMonths()
    {
        var data = await _orderService.GetAvailableYearsAndMonthsAsync();
        return Ok(data);
    }

}
