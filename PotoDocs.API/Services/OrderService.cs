using AutoMapper;
using Azure.Communication.Email;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.Shared.Models;
using System.Linq;

namespace PotoDocs.API.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(Guid id, OrderDto dto, IEnumerable<IFormFile> orderFiles, IEnumerable<IFormFile> cmrFiles, IEnumerable<Guid> fileIdsToDelete);
    Task<OrderDto> CreateAsync(OrderDto dto, IEnumerable<IFormFile> orderfiles, IEnumerable<IFormFile> cmrfiles);
    Task<OrderDto> ParseOrderAsync(IFormFile file);
    Task<OrderDto> ParseExistingOrderAsync(Guid id);

    Task<OrderFile> SaveFileAsync(IFormFile file, Guid orderId, FileType fileType);
    Task<(byte[] Bytes, string MimeType, string OriginalName)> GetFileAsync(Guid fileId);
    Task DeleteFileAsync(Guid fileId);
    Task SendDocumentsAsync(Guid orderId);
    Task MarkOrdersAsSentAsync(List<Guid> orderIds, DateTime sentDate);
}


public class OrderService : IOrderService
{
    private readonly PotodocsDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IOpenAIService _openAIService;
    private readonly IInvoiceService _invoiceService;
    private readonly IWebHostEnvironment _env;
    private readonly IEmailService _emailService;

    private readonly string _zleceniaFolderPath;
    private readonly string _cmrFolderPath;

    public OrderService(PotodocsDbContext dbContext, IMapper mapper, IOpenAIService openAIService, IInvoiceService invoiceService, IWebHostEnvironment env, IEmailService emailService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _openAIService = openAIService;
        _invoiceService = invoiceService;
        _emailService = emailService;
        _env = env;

        _zleceniaFolderPath = Path.Combine(_env.ContentRootPath, "PrivateFiles", "zlecenia");
        _cmrFolderPath = Path.Combine(_env.ContentRootPath, "PrivateFiles", "cmr");

        Directory.CreateDirectory(_zleceniaFolderPath);
        Directory.CreateDirectory(_cmrFolderPath);
    }

    public async Task<OrderDto> CreateAsync(OrderDto dto, IEnumerable<IFormFile> orderfiles, IEnumerable<IFormFile> cmrfiles)
    {
        var order = _mapper.Map<Order>(dto);

        if (dto.Driver?.Email != null)
        {
            order.Driver = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == dto.Driver.Email);
        }

        _dbContext.Orders.Add(order);

        foreach (var file in orderfiles)
            await SaveFileAsync(file, order.Id, FileType.Order);

        foreach (var file in cmrfiles)
            await SaveFileAsync(file, order.Id, FileType.Cmr);

        await _dbContext.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }
    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var query = _dbContext.Orders
            .Include(o => o.Driver)
            .Include(o => o.Files)
            .Include(o => o.Company)
            .Include(o => o.Stops)
            .Include(o => o.Invoice)
            .AsNoTracking();

        query = query.OrderByDescending(o => o.UnloadingDate.Year)
                        .ThenByDescending(o => o.UnloadingDate.Month);


        var orders = await query.ToListAsync();

        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> GetByIdAsync(Guid id)
    {
        var order = await _dbContext.Orders
          .Include(o => o.Driver)
          .Include(o => o.Files)
          .Include(o => o.Company)
          .Include(o => o.Stops)
          .FirstOrDefaultAsync(o => o.Id == id) ?? throw new KeyNotFoundException("Nie znaleziono zlecenia.");
        return _mapper.Map<OrderDto>(order);
    }

    public async Task UpdateAsync(Guid id, OrderDto dto, IEnumerable<IFormFile> orderFiles, IEnumerable<IFormFile> cmrFiles, IEnumerable<Guid> fileIdsToDelete)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var orderToUpdate = await _dbContext.Orders
                .Include(o => o.Files)
                .Include(o => o.Stops)
                .SingleOrDefaultAsync(o => o.Id == id) ?? throw new KeyNotFoundException("Nie znaleziono zlecenia do aktualizacji.");
            if (fileIdsToDelete != null)
            {
                var filesToRemove = orderToUpdate.Files
                    .Where(f => fileIdsToDelete.Contains(f.Id))
                    .ToList();

                foreach (var file in filesToRemove)
                {
                    var fullDiskPath = Path.Combine(file.Path, $"{file.Id}{file.Extension}");
                    if (File.Exists(fullDiskPath))
                    {
                        File.Delete(fullDiskPath);
                    }

                    _dbContext.OrderFiles.Remove(file);
                }
            }

            _mapper.Map(dto, orderToUpdate);

            if (dto.Driver?.Email != null)
            {
                orderToUpdate.Driver = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == dto.Driver.Email);
            }
            else
            {
                orderToUpdate.Driver = null;
            }

            foreach (var file in orderFiles)
                await SaveFileAsync(file, orderToUpdate.Id, FileType.Order);

            foreach (var file in cmrFiles)
                await SaveFileAsync(file, orderToUpdate.Id, FileType.Cmr);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await _dbContext.Orders
          .Include(o => o.Files)
          .FirstOrDefaultAsync(o => o.Id == id) ?? throw new KeyNotFoundException("Zlecenie nie istnieje.");
        var filesToDelete = order.Files.ToList();

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync();

        foreach (var file in filesToDelete)
        {
            var fullDiskPath = Path.Combine(file.Path, $"{file.Id}{file.Extension}");
            if (File.Exists(fullDiskPath))
            {
                File.Delete(fullDiskPath);
            }
        }
    }

    public async Task<OrderDto> ParseOrderAsync(IFormFile file)
    {
        return await ProcessOrderParsingAsync(file);
    }

    public async Task<OrderDto> ParseExistingOrderAsync(Guid id)
    {
        var (fileBytes, mimeType, originalName) = await GetFileAsync(id);

        var memoryStream = new MemoryStream(fileBytes);

        IFormFile formFile = new FormFile(memoryStream, 0, fileBytes.Length, "file", originalName)
        {
            Headers = new HeaderDictionary(),
            ContentType = mimeType
        };

        return await ProcessOrderParsingAsync(formFile);
    }

    private async Task<OrderDto> ProcessOrderParsingAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new BadHttpRequestException("Plik jest pusty lub nieprawidłowy.");
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadHttpRequestException("Dozwolone są tylko pliki PDF.");
        }

        var order = await _openAIService.GetInfoFromText(file);

        var firstUnloadingStop = order.Stops
            .Where(stop => stop.Type == StopType.Unloading)
            .OrderBy(stop => stop.Date)
            .FirstOrDefault();

        var issueDate = firstUnloadingStop?.Date ?? DateTime.Now;
        order.UnloadingDate = issueDate;

        return order;
    }

    private string GetStoragePath(FileType fileType)
    {
        return fileType == FileType.Order ? _zleceniaFolderPath : _cmrFolderPath;
    }

    public async Task<OrderFile> SaveFileAsync(IFormFile file, Guid orderId, FileType fileType)
    {
        var extension = Path.GetExtension(file.FileName);

        var fileEntity = new OrderFile
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Type = fileType,
            Name = Path.GetFileNameWithoutExtension(file.FileName),
            Size = file.Length,
            Extension = extension.TrimStart('.'),
            MimeType = file.ContentType,
            Path = GetStoragePath(fileType)
        };

        var fullDiskPath = Path.Combine(fileEntity.Path, $"{fileEntity.Id}{extension}");

        await using (var stream = new FileStream(fullDiskPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _dbContext.OrderFiles.Add(fileEntity);

        return fileEntity;
    }

    public async Task<(byte[] Bytes, string MimeType, string OriginalName)> GetFileAsync(Guid fileId)
    {
        var fileEntity = await _dbContext.OrderFiles.FindAsync(fileId) ?? throw new FileNotFoundException("File not found in database.");
        var fullDiskPath = Path.Combine(fileEntity.Path, $"{fileEntity.Id}.{fileEntity.Extension}");

        if (!File.Exists(fullDiskPath))
            throw new FileNotFoundException("File not found on disk.");

        var bytes = await File.ReadAllBytesAsync(fullDiskPath);

        return (bytes, fileEntity.MimeType, fileEntity.Name);
    }

    public async Task DeleteFileAsync(Guid fileId)
    {
        var fileEntity = await _dbContext.OrderFiles.FindAsync(fileId) ?? throw new FileNotFoundException("File not found in database.");
        var fullDiskPath = Path.Combine(fileEntity.Path, $"{fileEntity.Id}{fileEntity.Extension}");

        _dbContext.OrderFiles.Remove(fileEntity);
        await _dbContext.SaveChangesAsync();

        if (File.Exists(fullDiskPath))
        {
            File.Delete(fullDiskPath);
        }
    }
    public async Task SendDocumentsAsync(Guid orderId)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Order)
                .ThenInclude(o => o!.Company)
            .Include(i => i.Order)
                .ThenInclude(o => o!.Files)
            .FirstOrDefaultAsync(i => i.OrderId == orderId)
            ?? throw new KeyNotFoundException($"Nie znaleziono faktury dla zlecenia.");

        if (string.IsNullOrWhiteSpace(invoice.Order!.Company.EmailAddress))
            throw new InvalidOperationException($"Firma {invoice.Order.Company.Name} nie ma zdefiniowanego adresu e-mail.");

        string subject = $"PotoDocs Documents for Order: {invoice.Order.OrderNumber ?? invoice.InvoiceNumber.ToString()}";
        string emailTo = invoice.Order.Company.EmailAddress;

        try
        {
            var attachmentsList = new List<EmailAttachment>();

            var (fakturaPdfBytes, _, fakturaName) = await _invoiceService.GetInvoiceAsync(invoice.Id);
            attachmentsList.Add(new EmailAttachment(
                fakturaName,
                "application/pdf",
                new BinaryData(fakturaPdfBytes)
            ));

            var cmrFiles = invoice.Order.Files.Where(f => f.Type == FileType.Cmr).ToList();

            foreach (var cmrFile in cmrFiles)
            {
                var (bytes, mimeType, originalName) = await GetFileAsync(cmrFile.Id);
                var fileName = $"{originalName}.{cmrFile.Extension}";

                attachmentsList.Add(new EmailAttachment(
                    fileName,
                    mimeType,
                    new BinaryData(bytes)
                ));
            }

            string htmlBody = await LoadAndFormatEmailTemplate(invoice);

            await _emailService.SendEmailAsync(
                toEmail: emailTo,
                subject: subject,
                htmlContent: htmlBody,
                plainTextContent: $"Please find the attached documents for your order {invoice.Order.OrderNumber}.",
                attachments: attachmentsList
            );

            invoice.SentDate = DateTime.Now;
            invoice.DeliveryMethod = DeliveryMethodType.Email;

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<string> LoadAndFormatEmailTemplate(Invoice invoice)
    {
        var templatePath = Path.Combine(_env.WebRootPath, "emails", "invoice-documents.html");
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("Nie znaleziono szablonu 'invoice-documents.html'", templatePath);
        }

        var template = await File.ReadAllTextAsync(templatePath);

        template = template.Replace("{clientName}", invoice.Order!.Company.Name);
        template = template.Replace("{orderNumber}", invoice.Order.OrderNumber);
        template = template.Replace("{invoiceNumber}", $"{invoice.InvoiceNumber}/{invoice.IssueDate:MM'/'yyyy}");

        return template;
    }
    public async Task MarkOrdersAsSentAsync(List<Guid> orderIds, DateTime sentDate)
    {
        if (orderIds == null || orderIds.Count == 0) return;

        await _dbContext.Invoices
            .Where(i => i.OrderId.HasValue && orderIds.Contains(i.OrderId.Value))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.SentDate, sentDate)
                .SetProperty(i => i.DeliveryMethod, DeliveryMethodType.Post));
    }
}
