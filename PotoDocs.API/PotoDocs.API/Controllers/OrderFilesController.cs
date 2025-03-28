using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;

[Route("api/orders")]
[ApiController]
[Authorize(Roles = "admin,manager")]
public class OrderFilesController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderFilesController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("{id}/cmr")]
    public async Task<ActionResult> UploadCMR(Guid id, [FromForm] List<IFormFile> files)
    {
        var updatedOrder = await _orderService.AddCmr(files, id);
        return Ok(updatedOrder); // albo CreatedAtAction z ID, jeśli chcesz
    }

    [HttpDelete("{id}/cmr/{fileName}")]
    public IActionResult DeleteCMR(string fileName)
    {
        _orderService.DeleteCmr(fileName);
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

    [HttpGet("{id}/invoice")]
    public async Task<IActionResult> GetInvoiceAsync(Guid id)
    {
        var pdfData = await _orderService.GetPdf(id);

        if (pdfData == null || pdfData.Length == 0)
            return NotFound("Nie udało się wygenerować faktury.");

        return File(pdfData, "application/pdf", $"faktura-{id}.pdf");
    }

    [HttpGet("invoices/{year}/{month}")]
    public async Task<IActionResult> GetInvoices(int year, int month)
    {
        var zipData = await _orderService.GetZip(year, month);

        if (zipData == null || zipData.Length == 0)
            return NotFound("Brak faktur dla danego miesiąca.");

        return File(zipData, "application/zip", $"faktury-{month:D2}-{year}.zip");
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetAvailableYearsAndMonths()
    {
        var data = await _orderService.GetAvailableYearsAndMonthsAsync();
        return Ok(data);
    }
}
