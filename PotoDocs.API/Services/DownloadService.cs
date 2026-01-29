using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using System.IO.Compression;

namespace PotoDocs.API.Services;

public interface IDownloadService
{
    Task<FileStream> GetDocumentsArchiveAsync(List<Guid> orderIds);
    Task<FileStream> GetOrdersArchiveAsync(List<Guid> orderIds);
    Task<FileStream> GetInvoicesArchiveAsync(List<Guid> invoiceIds);
}

public class DownloadService(PotodocsDbContext dbContext, IInvoiceService invoiceService) : IDownloadService
{
    private readonly PotodocsDbContext _dbContext = dbContext;
    private readonly IInvoiceService _invoiceService = invoiceService;

    private static FileStream CreateTempFileStream()
    {
        var tempFilePath = Path.GetTempFileName();
        return new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }

    public async Task<FileStream> GetDocumentsArchiveAsync(List<Guid> orderIds)
    {
        var invoices = await _dbContext.Invoices
                .Where(i => i.OrderId.HasValue && orderIds.Contains(i.OrderId.Value))
                .Include(i => i.Items)
                .Include(i => i.Order)
                .Include(i => i.Order!.Files)
                .AsNoTracking()
                .ToListAsync();

        if (invoices == null || invoices.Count == 0)
            throw new KeyNotFoundException("Brak plików dla zleceń wybranego miesiąca.");

        var fileStream = CreateTempFileStream();

        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var invoice in invoices)
            {
                string folderName = $"zlecenie {invoice.InvoiceNumber}";

                var pdfData = await _invoiceService.GetInvoiceFileAsync(invoice.Id);

                var pdfZipEntry = archive.CreateEntry($"{folderName}/{pdfData.OriginalName}", CompressionLevel.Optimal);
                using (var pdfEntryStream = pdfZipEntry.Open())
                {
                    await pdfEntryStream.WriteAsync(pdfData.Bytes);
                }

                if (invoice.Order?.Files != null)
                {
                    foreach (var file in invoice.Order.Files)
                    {
                        var fullDiskPath = Path.Combine(file.Path, $"{file.Id}.{file.Extension}");

                        if (!File.Exists(fullDiskPath)) continue;

                        string fileInZipName = $"{file.Name}.{file.Extension}";
                        var fileZipEntry = archive.CreateEntry($"{folderName}/{fileInZipName}", CompressionLevel.Optimal);

                        using var fileEntryStream = fileZipEntry.Open();
                        using var sourceFileStream = File.OpenRead(fullDiskPath);
                        await sourceFileStream.CopyToAsync(fileEntryStream);
                    }
                }
            }
        }

        fileStream.Position = 0;
        return fileStream;
    }

    public async Task<FileStream> GetOrdersArchiveAsync(List<Guid> orderIds)
    {
        var orders = await _dbContext.Orders
            .Where(o => orderIds.Contains(o.Id))
            .Include(o => o.Files)
            .ToListAsync();

        if (orders == null || orders.Count == 0)
            throw new KeyNotFoundException("Brak plików dla zleceń wybranego miesiąca.");

        var fileStream = CreateTempFileStream();

        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var order in orders)
            {
                foreach (var file in order.Files)
                {
                    var fullDiskPath = Path.Combine(file.Path, $"{file.Id}.{file.Extension}");

                    if (!File.Exists(fullDiskPath)) continue;

                    var zipEntry = archive.CreateEntry($"{order.UnloadingDate:MM-yyyy}/{file.Name}.{file.Extension}", CompressionLevel.Optimal);

                    using var entryStream = zipEntry.Open();
                    using var sourceFileStream = File.OpenRead(fullDiskPath);
                    await sourceFileStream.CopyToAsync(entryStream);
                }
            }
        }

        fileStream.Position = 0;
        return fileStream;
    }

    public async Task<FileStream> GetInvoicesArchiveAsync(List<Guid> invoiceIds)
    {
        var invoices = await _dbContext.Invoices
            .Where(i => invoiceIds.Contains(i.Id))
            .Include(i => i.Items)
            .Include(i => i.Order)
            .AsNoTracking()
            .ToListAsync();

        if (invoices == null || invoices.Count == 0)
            throw new KeyNotFoundException("Brak faktur dla wybranego miesiąca.");

        var fileStream = CreateTempFileStream();

        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var invoice in invoices)
            {
                var pdfData = await _invoiceService.GetInvoiceFileAsync(invoice.Id);

                var zipEntry = archive.CreateEntry(pdfData.OriginalName, CompressionLevel.Optimal);

                using var entryStream = zipEntry.Open();
                await entryStream.WriteAsync(pdfData.Bytes);
            }
        }

        fileStream.Position = 0;
        return fileStream;
    }
}