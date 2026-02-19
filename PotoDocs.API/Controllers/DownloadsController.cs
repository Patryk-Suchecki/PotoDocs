using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin,manager")]
public class DownloadsController(IDownloadService downloadService) : ControllerBase
{
    private readonly IDownloadService _downloadService = downloadService;

    [HttpPost("documents")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadDocuments([FromBody] DownloadRequest request)
    {
        var result = await _downloadService.GetDocumentsArchiveAsync(request.Ids);

        return File(result.FileStream, result.ContentType, result.FileName);
    }

    [HttpPost("invoices")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadInvoices([FromBody] DownloadRequest request)
    {
        var result = await _downloadService.GetInvoicesArchiveAsync(request.Ids);
        return File(result.FileStream, result.ContentType, result.FileName);
    }

    [HttpPost("orders")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadOrders([FromBody] DownloadRequest request)
    {
        var result = await _downloadService.GetOrdersArchiveAsync(request.Ids);
        return File(result.FileStream, result.ContentType, result.FileName);
    }
}