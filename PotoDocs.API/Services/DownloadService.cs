using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.Shared.Models;
using System.IO.Compression;
using System.Text;

namespace PotoDocs.API.Services;

public interface IDownloadService
{
    Task<FileDownloadResult> GetDocumentsArchiveAsync(List<Guid> orderIds);
    Task<FileDownloadResult> GetOrdersArchiveAsync(List<Guid> orderIds);
    Task<FileDownloadResult> GetInvoicesArchiveAsync(List<Guid> invoiceIds);
}

public class DownloadService(PotodocsDbContext dbContext, IInvoiceService invoiceService, IFileStorageService fileStorage) : IDownloadService
{
    private readonly PotodocsDbContext _dbContext = dbContext;
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<FileDownloadResult> GetInvoicesArchiveAsync(List<Guid> invoiceIds)
    {
        var invoices = await _dbContext.Invoices
            .Where(i => invoiceIds.Contains(i.Id))
            .AsNoTracking()
            .ToListAsync();

        if (invoices.Count == 0) throw new KeyNotFoundException("Nie znaleziono wybranych faktur.");

        return await GenerateZipArchiveAsync(
            $"FAKTURY_{DateTime.Now:yyyy-MM-dd}.zip",
            async (archive) =>
            {
                foreach (var invoice in invoices)
                {
                    await AddInvoiceToArchiveAsync(archive, invoice, rootFolder: null);
                }
            });
    }

    public async Task<FileDownloadResult> GetOrdersArchiveAsync(List<Guid> orderIds)
    {
        var orders = await GetOrdersWithDetailsAsync(orderIds);
        if (orders.Count == 0) throw new KeyNotFoundException("Nie znaleziono wybranych zleceń.");

        return await GenerateZipArchiveAsync(
            $"ZLECENIA_{DateTime.Now:yyyy-MM-dd}.zip",
            async (archive) =>
            {
                foreach (var order in orders)
                {
                    string rootFolder = GetOrderRootFolderName(order);
                    await AddOrderFilesToArchiveAsync(archive, order, rootFolder);
                }
            });
    }

    public async Task<FileDownloadResult> GetDocumentsArchiveAsync(List<Guid> orderIds)
    {
        var orders = await GetOrdersWithDetailsAsync(orderIds);
        if (orders.Count == 0) throw new KeyNotFoundException("Nie znaleziono dokumentów.");

        var invoices = await _dbContext.Invoices
            .Where(i => i.OrderId.HasValue && orderIds.Contains(i.OrderId.Value))
            .AsNoTracking()
            .ToListAsync();

        var invoicesByOrder = invoices.ToLookup(i => i.OrderId!.Value);

        return await GenerateZipArchiveAsync(
            $"KOMPLET_{DateTime.Now:yyyy-MM-dd}.zip",
            async (archive) =>
            {
                foreach (var order in orders)
                {
                    string rootFolder = GetOrderRootFolderName(order);

                    await AddOrderFilesToArchiveAsync(archive, order, rootFolder);

                    foreach (var invoice in invoicesByOrder[order.Id])
                    {
                        await AddInvoiceToArchiveAsync(archive, invoice, rootFolder);
                    }
                }
            });
    }

    private static async Task<FileDownloadResult> GenerateZipArchiveAsync(string fileName, Func<ZipArchive, Task> populateAction)
    {
        var fileStream = CreateTempFileStream();

        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            await populateAction(archive);
        }

        fileStream.Position = 0;
        return new FileDownloadResult(fileStream, fileName, "application/zip");
    }

    private static FileStream CreateTempFileStream()
    {
        var temp = Path.GetTempFileName();
        return new FileStream(temp, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }

    private async Task<List<Order>> GetOrdersWithDetailsAsync(List<Guid> orderIds)
    {
        return await _dbContext.Orders
            .Where(o => orderIds.Contains(o.Id))
            .Include(o => o.Files)
            .Include(o => o.Company)
            .AsNoTracking()
            .ToListAsync();
    }

    private async Task AddOrderFilesToArchiveAsync(ZipArchive archive, Order order, string rootFolder)
    {
        if (order.Files == null) return;

        foreach (var file in order.Files)
        {
            string? subFolder = file.Type switch
            {
                FileType.Order => "Zlecenia",
                FileType.Cmr => "CMR",
                _ => null
            };

            if (subFolder is null) continue;

            string entryPath = $"{rootFolder}/{subFolder}/{file.Name}{file.Extension}";
            string fileNameOnDisk = $"{file.Id}{file.Extension}";

            await TryAddStorageFileToArchiveAsync(archive, entryPath, file.Path, fileNameOnDisk);
        }
    }

    private async Task AddInvoiceToArchiveAsync(ZipArchive archive, Invoice invoice, string? rootFolder)
    {
        string typeFolder = invoice.Type == InvoiceType.Correction ? "Korekty" : "Faktury";

        string fileName = invoice.Type == InvoiceType.Correction
            ? $"KOREKTA_{invoice.SafeFileName}.pdf"
            : $"FAKTURA_{invoice.SafeFileName}.pdf";

        string entryPath = rootFolder != null
            ? $"{rootFolder}/Dokumenty/{typeFolder}/{fileName}"
            : $"{invoice.IssueDate:MM-yyyy}/{typeFolder}/{fileName}";

        try
        {
            var result = await _invoiceService.GetInvoiceStreamAsync(invoice.Id);

            using var sourceStream = result.FileStream;
            await AddStreamToArchiveAsync(archive, entryPath, sourceStream);
        }
        catch (Exception ex)
        {
            string errorInfo = $"Błąd generowania PDF dla faktury {invoice.InvoiceNumber}: {ex.Message}";
            await AddBytesToArchiveAsync(archive, $"{entryPath}.BLAD.txt", Encoding.UTF8.GetBytes(errorInfo));
        }
    }

    private async Task TryAddStorageFileToArchiveAsync(ZipArchive archive, string entryName, string folderPath, string fileNameOnDisk)
    {
        try
        {
            var (fileStream, _) = await _fileStorage.GetFileStreamAsync(folderPath, fileNameOnDisk);
            using (fileStream)
            {
                await AddStreamToArchiveAsync(archive, entryName, fileStream);
            }
        }
        catch (FileNotFoundException)
        {
            string msg = $"Plik fizyczny nie istnieje: {fileNameOnDisk}";
            await AddBytesToArchiveAsync(archive, $"{entryName}.BRAK.txt", Encoding.UTF8.GetBytes(msg));
        }
        catch (Exception)
        {
            await AddBytesToArchiveAsync(archive, $"{entryName}.BLAD.txt", Encoding.UTF8.GetBytes("Błąd odczytu pliku."));
        }
    }
    private static async Task AddStreamToArchiveAsync(ZipArchive archive, string entryName, Stream sourceStream)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        await sourceStream.CopyToAsync(entryStream);
    }
    private static async Task AddBytesToArchiveAsync(ZipArchive archive, string entryName, byte[] data)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        await entryStream.WriteAsync(data);
    }

    private static string GetOrderRootFolderName(Order order)
    {
        if (!string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            return $"Zlecenie {SanitizeFileName(order.OrderNumber)}";
        }

        var companyName = order.Company?.Name ?? "NieznanaFirma";
        return $"Zlecenie_BezNumeru_{SanitizeFileName(companyName)}";
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }
}