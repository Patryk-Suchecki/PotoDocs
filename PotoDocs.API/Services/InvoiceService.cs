using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Extensions;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllAsync();
    Task<InvoiceDto> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(InvoiceDto dto);
    Task<InvoiceDto> CreateAsync(InvoiceDto dto);
    Task<(byte[] Bytes, string MimeType, string OriginalName)> GetInvoiceFileAsync(Guid id);
    Task<InvoiceDto> CreateFromOrderAsync(Guid orderId);
    Task<InvoiceDto> CreateCorrectionAsync(InvoiceCorrectionDto dto);
    Task UpdateCorrectionAsync(InvoiceCorrectionDto dto);
}

public class InvoiceService(PotodocsDbContext dbContext, IMapper mapper, IInvoicePdfGenerator pdfGenerator, IEuroRateService euroRateService, IInvoiceNumberGenerator numberGenerator) : IInvoiceService
{
    private readonly PotodocsDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IInvoicePdfGenerator _pdfGenerator = pdfGenerator;
    private readonly IEuroRateService _euroRateService = euroRateService;
    private readonly IInvoiceNumberGenerator _numberGenerator = numberGenerator;

    public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
    {
        var invoice = _mapper.Map<Invoice>(dto);
        invoice.RecalculateTotals();
        invoice.InvoiceNumber = await _numberGenerator.GetNextNumberAsync(invoice.IssueDate, InvoiceType.Original);

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
    {
        var invoices = await _dbContext.Invoices
            .IncludeFullDetails()
            .AsNoTracking()
            .OrderByDescending(o => o.IssueDate.Year)
            .ThenByDescending(o => o.IssueDate.Month)
            .ThenByDescending(o => o.InvoiceNumber)
            .ToListAsync();

        return _mapper.Map<List<InvoiceDto>>(invoices);
    }

    public async Task<InvoiceDto> GetByIdAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices
            .IncludeFullDetails()
            .FirstOrThrowAsync(o => o.Id == id);

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task UpdateAsync(InvoiceDto dto)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrThrowAsync(o => o.Id == dto.Id);

        _mapper.Map(dto, invoice);
        SyncInvoiceItems(invoice, dto.Items);
        invoice.RecalculateTotals();

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices
            .FirstOrThrowAsync(o => o.Id == id);

        _dbContext.Invoices.Remove(invoice);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(byte[] Bytes, string MimeType, string OriginalName)> GetInvoiceFileAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices
                .IncludeFullDetails()
                .FirstOrThrowAsync(i => i.Id == id);

        EuroRateDto? euroRate = null;
        if (invoice.Currency != CurrencyType.PLN)
        {
            euroRate = await _euroRateService.GetEuroRateAsync(invoice.SaleDate);
        }

        var pdfBytes = await _pdfGenerator.GenerateAsync(invoice, euroRate);
        var fileName = $"FAKTURA_{invoice.InvoiceNumber}-{invoice.IssueDate:MM'-'yyyy}.pdf";

        return (pdfBytes, "application/pdf", fileName);
    }

    public async Task<InvoiceDto> CreateFromOrderAsync(Guid orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Company)
            .FirstOrThrowAsync(o => o.Id == orderId);

        if (await _dbContext.Invoices.AnyAsync(i => i.OrderId == orderId))
            throw new InvalidOperationException("Faktura do tego zlecenia została już wystawiona.");

        decimal vatRate = order.Company.Country == "PL" ? 0.23m : 0m;
        decimal netAmount = order.Price;

        var invoice = new Invoice
        {
            OrderId = order.Id,
            InvoiceNumber = await _numberGenerator.GetNextNumberAsync(DateTime.UtcNow, InvoiceType.Original),
            IssueDate = DateTime.Now,
            SaleDate = order.UnloadingDate,
            BuyerName = order.Company.Name,
            BuyerAddress = order.Company.Address,
            BuyerNIP = order.Company.NIP,
            PaymentMethod = "Przelew",
            PaymentDeadlineDays = order.PaymentDeadline,
            Currency = order.Currency,
            Comments = $"Zlecenie nr {order.OrderNumber}",
            Items = []
        };

        var item = new InvoiceItem
        {
            Name = "Usługa transportowa",
            Unit = "Usługa",
            Quantity = 1,
            NetPrice = netAmount,
            VatRate = vatRate,
        };

        invoice.AddItem(item);

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto> CreateCorrectionAsync(InvoiceCorrectionDto dto)
    {
        var history = await _dbContext.Invoices
            .Where(i => i.Id == dto.OriginalInvoice.Id || i.OriginalInvoiceId == dto.OriginalInvoice.Id)
            .AsNoTracking()
            .ToListAsync();

        var original = history.FirstOrDefault(i => i.Type == InvoiceType.Original)
            ?? throw new KeyNotFoundException("Nie znaleziono faktury pierwotnej.");

        if (original.Id != dto.OriginalInvoice.Id && history.Any(h => h.Id == dto.OriginalInvoice.Id && h.Type == InvoiceType.Correction))
        {
            throw new InvalidOperationException("Należy korygować zawsze fakturę pierwotną.");
        }

        var correction = _mapper.Map<Invoice>(original);

        correction.Type = InvoiceType.Correction;
        correction.OriginalInvoiceId = original.Id;
        correction.OrderId = null;
        correction.InvoiceNumber = await _numberGenerator.GetNextNumberAsync(dto.IssueDate ?? DateTime.Now, InvoiceType.Correction);
        correction.IssueDate = dto.IssueDate ?? DateTime.Now;
        correction.Comments = dto.Comments;
        correction.DeliveryMethod = dto.DeliveryMethod ?? correction.DeliveryMethod;
        correction.SentDate = dto.SentDate;
        correction.HasPaid = dto.HasPaid;

        correction.Items = _mapper.Map<List<InvoiceItem>>(dto.Items);

        foreach (var item in correction.Items)
        {
            item.InvoiceId = correction.Id;
            item.CalculateRow();
        }

        correction.RecalculateTotals();

        _dbContext.Invoices.Add(correction);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(correction);
    }

    public async Task UpdateCorrectionAsync(InvoiceCorrectionDto dto)
    {
        var correction = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrThrowAsync(i => i.Id == dto.Id);

        correction.IssueDate = dto.IssueDate ?? DateTime.Now;
        correction.Comments = dto.Comments;
        correction.DeliveryMethod = dto.DeliveryMethod;
        correction.SentDate = dto.SentDate;
        correction.HasPaid = dto.HasPaid;
        correction.InvoiceNumber = dto.InvoiceNumber;

        SyncInvoiceItems(correction, dto.Items);
        correction.RecalculateTotals();

        await _dbContext.SaveChangesAsync();
    }

    private void SyncInvoiceItems(Invoice invoice, ICollection<InvoiceItemDto> incomingItems)
    {
        var incomingIds = incomingItems.Select(x => x.Id).ToHashSet();
        var itemsToDelete = invoice.Items.Where(x => !incomingIds.Contains(x.Id)).ToList();

        if (itemsToDelete.Count != 0)
        {
            _dbContext.Set<InvoiceItem>().RemoveRange(itemsToDelete);
            foreach (var item in itemsToDelete) invoice.Items.Remove(item);
        }

        foreach (var dtoItem in incomingItems)
        {
            var existingItem = invoice.Items.FirstOrDefault(x => x.Id == dtoItem.Id);

            if (existingItem != null)
            {
                _mapper.Map(dtoItem, existingItem);
                existingItem.CalculateRow();
            }
            else
            {
                var newItem = _mapper.Map<InvoiceItem>(dtoItem);
                if (newItem.Id == Guid.Empty) newItem.Id = Guid.NewGuid();
                invoice.Items.Add(newItem);
            }
        }
    }
}