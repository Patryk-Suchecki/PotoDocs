using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Exceptions;
using PotoDocs.API.Extensions;
using PotoDocs.API.Utils;
using PotoDocs.Shared.Models;

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
    Task<FileDownloadResult> GetFileStreamAsync(Guid fileId);
    Task<OrderFile> SaveFileAsync(IFormFile file, Guid orderId, FileType fileType);
    Task DeleteFileAsync(Guid fileId);
    Task SendDocumentsAsync(Guid orderId);
    Task MarkOrdersAsSentAsync(List<Guid> orderIds, DateTime sentDate);
}

public class OrderService(PotodocsDbContext dbContext, IMapper mapper, IOpenAIService openAIService, IInvoiceService invoiceService, IFileStorageService fileStorage, IOrderDocumentSender documentSender) : IOrderService
{
    private readonly PotodocsDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IOpenAIService _openAIService = openAIService;
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly IFileStorageService _fileStorage = fileStorage;
    private readonly IOrderDocumentSender _documentSender = documentSender;

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _dbContext.Orders
            .IncludeFullDetails()
            .AsNoTracking()
            .OrderByDescending(o => o.UnloadingDate.Year)
            .ThenByDescending(o => o.UnloadingDate.Month)
            .ToListAsync();

        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> GetByIdAsync(Guid id)
    {
        var order = await _dbContext.Orders
            .IncludeFullDetails()
            .FirstOrThrowAsync(o => o.Id == id);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> CreateAsync(OrderDto dto, IEnumerable<IFormFile> orderFiles, IEnumerable<IFormFile> cmrFiles)
    {
        await using var fileScope = new FileTransactionScope(_fileStorage);
        var order = _mapper.Map<Order>(dto);

        await AssignDriverAsync(order, dto.Driver?.Email);

        _dbContext.Orders.Add(order);

        await SaveAndTrackFilesAsync(orderFiles, order.Id, FileType.Order, fileScope);
        await SaveAndTrackFilesAsync(cmrFiles, order.Id, FileType.Cmr, fileScope);

        await _dbContext.SaveChangesAsync();

        fileScope.Complete();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task UpdateAsync(Guid id, OrderDto dto, IEnumerable<IFormFile> orderFiles, IEnumerable<IFormFile> cmrFiles, IEnumerable<Guid> fileIdsToDelete)
    {
        var filesPendingPhysicalDeletion = new List<OrderFile>();
        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
        await using var fileScope = new FileTransactionScope(_fileStorage);

        try
        {
            var order = await _dbContext.Orders
                .Include(o => o.Files)
                .Include(o => o.Stops)
                .FirstOrThrowAsync(o => o.Id == id);

            if (fileIdsToDelete != null && fileIdsToDelete.Any())
            {
                var filesToRemove = order.Files.Where(f => fileIdsToDelete.Contains(f.Id)).ToList();
                if (filesToRemove.Count != 0)
                {
                    _dbContext.OrderFiles.RemoveRange(filesToRemove);
                    filesPendingPhysicalDeletion.AddRange(filesToRemove);
                }
            }

            _mapper.Map(dto, order);
            await AssignDriverAsync(order, dto.Driver?.Email);

            await SaveAndTrackFilesAsync(orderFiles, order.Id, FileType.Order, fileScope);
            await SaveAndTrackFilesAsync(cmrFiles, order.Id, FileType.Cmr, fileScope);

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            fileScope.Complete();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }

        foreach (var file in filesPendingPhysicalDeletion)
        {
            try { _fileStorage.DeleteFile(file.Path, $"{file.Id}{file.Extension}"); } catch { }
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Files)
            .FirstOrThrowAsync(o => o.Id == id);

        var filesToDelete = order.Files.ToList();

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync();

        foreach (var file in filesToDelete)
        {
            _fileStorage.DeleteFile(file.Path, $"{file.Id}{file.Extension}");
        }
    }

    private async Task SaveAndTrackFilesAsync(IEnumerable<IFormFile> files, Guid orderId, FileType type, FileTransactionScope scope)
    {
        foreach (var file in files)
        {
            var savedFile = await SaveFileAsync(file, orderId, type);

            scope.RegisterCreatedFile(savedFile.Path, $"{savedFile.Id}{savedFile.Extension}");
        }
    }

    private async Task AssignDriverAsync(Order order, string? email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            order.Driver = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
        }
        else
        {
            order.Driver = null;
        }
    }

    public async Task<OrderDto> ParseOrderAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("Plik jest pusty.");

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Tylko pliki PDF są obsługiwane.");

        var orderDto = await _openAIService.GetInfoFromText(file);

        orderDto.UnloadingDate = orderDto.Stops?
            .Where(s => s.Type == StopType.Unloading)
            .OrderBy(s => s.Date)
            .Select(s => s.Date)
            .FirstOrDefault() ?? DateTime.Now;

        return orderDto;
    }

    public async Task<OrderDto> ParseExistingOrderAsync(Guid id)
    {
        var result = await GetFileStreamAsync(id);
        using var memoryStream = new MemoryStream();
        await result.FileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "file", result.FileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = result.ContentType
        };

        return await ParseOrderAsync(formFile);
    }

    public async Task<OrderFile> SaveFileAsync(IFormFile file, Guid orderId, FileType fileType)
    {
        var fileId = Guid.NewGuid();
        var storedPath = await _fileStorage.SaveFileAsync(file, fileType, fileId);

        var fileEntity = new OrderFile
        {
            Id = fileId,
            OrderId = orderId,
            Type = fileType,
            Name = Path.GetFileNameWithoutExtension(file.FileName),
            Size = file.Length,
            Extension = Path.GetExtension(file.FileName),
            MimeType = file.ContentType,
            Path = storedPath
        };

        _dbContext.OrderFiles.Add(fileEntity);
        return fileEntity;
    }

    public async Task<FileDownloadResult> GetFileStreamAsync(Guid fileId)
    {
        var fileEntity = await _dbContext.OrderFiles
            .FirstOrThrowAsync(f => f.Id == fileId, "Nie znaleziono pliku.");

        var fileNameOnDisk = $"{fileEntity.Id}{fileEntity.Extension}";
        var (stream, contentType) = await _fileStorage.GetFileStreamAsync(fileEntity.Path, fileNameOnDisk);

        var downloadName = $"{fileEntity.Name}{fileEntity.Extension}";

        var finalContentType = !string.IsNullOrEmpty(fileEntity.MimeType) ? fileEntity.MimeType : contentType;

        return new FileDownloadResult(stream, downloadName, finalContentType);
    }

    public async Task DeleteFileAsync(Guid fileId)
    {
        var fileEntity = await _dbContext.OrderFiles
            .FirstOrThrowAsync(f => f.Id == fileId, "Nie znaleziono pliku.");

        _dbContext.OrderFiles.Remove(fileEntity);
        await _dbContext.SaveChangesAsync();

        _fileStorage.DeleteFile(fileEntity.Path, $"{fileEntity.Id}{fileEntity.Extension}");
    }

    public async Task SendDocumentsAsync(Guid orderId)
    {
        var invoice = await _dbContext.Invoices
             .Include(i => i.Order).ThenInclude(o => o!.Company)
             .Include(i => i.Order).ThenInclude(o => o!.Files)
             .Include(i => i.Corrections)
             .FirstOrThrowAsync(i => i.OrderId == orderId, "Nie znaleziono faktury dla zlecenia.");

        await _documentSender.SendDocumentsViaEmailAsync(invoice);

        var sentDoc = invoice.Corrections.OrderByDescending(c => c.IssueDate).FirstOrDefault() ?? invoice;
        sentDoc.SentDate = DateTime.Now;
        sentDoc.DeliveryMethod = DeliveryMethodType.Email;

        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkOrdersAsSentAsync(List<Guid> orderIds, DateTime sentDate)
    {
        if (orderIds == null || orderIds.Count == 0) return;

        await _dbContext.Invoices
            .Where(i => i.OrderId.HasValue && orderIds.Contains(i.OrderId.Value))
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.SentDate, sentDate)
                .SetProperty(i => i.DeliveryMethod, DeliveryMethodType.Post));
    }
}