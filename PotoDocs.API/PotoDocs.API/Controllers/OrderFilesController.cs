using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Entities;
using PotoDocs.API.Services;

[Route("api/orders/{orderId}/files")]
[ApiController]
[Authorize(Roles = "admin,manager")]
public class OrderFilesController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderFilesController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("cmr")]
    public async Task<ActionResult> UploadCMR(int orderId, [FromForm] List<IFormFile> files)
    {
        var response = await _orderService.AddCMRFileAsync(files, orderId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("cmr/{fileName}")]
    public ActionResult DeleteCMR(string fileName)
    {
        _orderService.DeleteCMR(fileName);
        return NoContent();
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

    [HttpGet("invoice")]
    public IActionResult GetInvoice(int orderId)
    {
        var pdfData = _orderService.CreateInvoicePDF(orderId);

        if (pdfData == null || pdfData.Length == 0)
        {
            return NotFound("Nie udało się wygenerować faktury.");
        }

        return File(pdfData, "application/pdf", $"Invoice_{orderId}.pdf");
    }
}
