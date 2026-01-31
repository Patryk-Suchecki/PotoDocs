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

public class DownloadService(PotodocsDbContext dbContext, IInvoiceService invoiceService, IFileStorageService fileStorage) : IDownloadService
{
    private readonly PotodocsDbContext _dbContext = dbContext;
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<FileStream> GetDocumentsArchiveAsync(List<Guid> orderIds)
    {
        var invoices = await _dbContext.Invoices
                .Where(i => i.OrderId.HasValue && orderIds.Contains(i.OrderId.Value))
                .Include(i => i.Order).ThenInclude(o => o!.Files)
                .AsNoTracking()
                .ToListAsync();

        if (invoices.Count == 0)
            throw new KeyNotFoundException("Brak dokumentów dla wybranych zleceń.");

        var fileStream = CreateTempFileStream();

        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var invoice in invoices)
            {
                string folderName = $"zlecenie {invoice.InvoiceNumber}";

                var pdfData = await _invoiceService.GetInvoiceFileAsync(invoice.Id);
                await AddBytesToArchiveAsync(archive, $"{folderName}/{pdfData.OriginalName}", pdfData.Bytes);

                if (invoice.Order?.Files != null)
                {
                    foreach (var file in invoice.Order.Files)
                    {
                        var fileNameInZip = $"{folderName}/{file.Name}.{file.Extension}";
                        var fileNameOnDisk = $"{file.Id}.{file.Extension}";

                        await TryAddStorageFileToArchiveAsync(archive, fileNameInZip, file.Path, fileNameOnDisk);
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
            .AsNoTracking()
            .ToListAsync();

        if (orders.Count == 0)
            throw new KeyNotFoundException("Brak plików dla wybranych zleceń.");

        var fileStream = CreateTempFileStream();

        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var order in orders)
            {
                foreach (var file in order.Files)
                {
                    var entryPath = $"{order.UnloadingDate:MM-yyyy}/{file.Name}.{file.Extension}";
                    var fileNameOnDisk = $"{file.Id}.{file.Extension}";

                    await TryAddStorageFileToArchiveAsync(archive, entryPath, file.Path, fileNameOnDisk);
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
            .AsNoTracking()
            .ToListAsync();

        if (invoices.Count == 0)
            throw new KeyNotFoundException("Brak faktur dla wybranego zakresu.");

        var fileStream = CreateTempFileStream();

        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var invoice in invoices)
            {
                var pdfData = await _invoiceService.GetInvoiceFileAsync(invoice.Id);
                await AddBytesToArchiveAsync(archive, pdfData.OriginalName, pdfData.Bytes);
            }
        }

        fileStream.Position = 0;
        return fileStream;
    }

    private static FileStream CreateTempFileStream()
    {
        var tempFilePath = Path.GetTempFileName();
        return new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }

    private static async Task AddBytesToArchiveAsync(ZipArchive archive, string entryName, byte[] data)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        await entryStream.WriteAsync(data);
    }

    private async Task TryAddStorageFileToArchiveAsync(ZipArchive archive, string entryName, string folderPath, string fileNameOnDisk)
    {
        var (bytes, _) = await _fileStorage.GetFileAsync(folderPath, fileNameOnDisk);

        await AddBytesToArchiveAsync(archive, entryName, bytes);
    }
}