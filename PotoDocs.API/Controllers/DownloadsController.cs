using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin,manager")]
public class DownloadsController(IDownloadService downloadService) : ControllerBase
{
    private readonly IDownloadService _downloadService = downloadService;

    [HttpPost("documents")]
    public async Task<IActionResult> DownloadDocuments([FromBody] List<Guid> orderIds)
    {
        if (orderIds == null || orderIds.Count == 0)
            return BadRequest("Nie wybrano żadnych zleceń do pobrania.");

        var fileStream = await _downloadService.GetDocumentsArchiveAsync(orderIds);

        var fileName = $"komplet_dokumentow_{DateTime.Now:yyyyMMdd_HHmm}.zip";

        return File(fileStream, "application/zip", fileName);
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> DownloadInvoices([FromBody] List<Guid> invoiceIds)
    {
        if (invoiceIds == null || invoiceIds.Count == 0)
            return BadRequest("Nie wybrano żadnych faktur do pobrania.");

        var fileStream = await _downloadService.GetInvoicesArchiveAsync(invoiceIds);
        var fileName = $"faktury_{DateTime.Now:yyyyMMdd_HHmm}.zip";

        return File(fileStream, "application/zip", fileName);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> DownloadOrders([FromBody] List<Guid> orderIds)
    {
        if (orderIds == null || orderIds.Count == 0)
            return BadRequest("Nie wybrano żadnych zleceń do pobrania.");

        var fileStream = await _downloadService.GetOrdersArchiveAsync(orderIds);
        var fileName = $"zlecenia_{DateTime.Now:yyyyMMdd_HHmm}.zip";

        return File(fileStream, "application/zip", fileName);
    }
}