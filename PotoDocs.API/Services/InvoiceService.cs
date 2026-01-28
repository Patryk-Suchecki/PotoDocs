using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PotoDocs.API.Entities;
using PotoDocs.API.Invoices;
using PotoDocs.API.Options;
using PotoDocs.Shared.Models;
using PotoDocs.Shared.Utils;
using QuestPDF.Fluent;
using System.Globalization;
using System.Xml.Linq;

namespace PotoDocs.API.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllAsync();
    Task<InvoiceDto> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(InvoiceDto dto);
    Task<InvoiceDto> CreateAsync(InvoiceDto dto);
    Task<byte[]> GenerateInvoicePdf(Invoice invoice);
    Task<(byte[] Bytes, string MimeType, string OriginalName)> GetInvoiceAsync(Guid id);
    Task<InvoiceDto> CreateFromOrderAsync(Guid orderId);
    Task<InvoiceDto> CreateCorrectionAsync(InvoiceCorrectionDto dto);
    Task UpdateCorrectionAsync(InvoiceCorrectionDto dto);
}

public class InvoiceService(PotodocsDbContext dbContext, IMapper mapper, IEuroRateService euroRateService, IOptions<OrganizationSettings> options) : IInvoiceService
{
    private readonly PotodocsDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IEuroRateService _euroRateService = euroRateService;

    private readonly OrganizationSettings _orgSettings = options.Value;

    public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
    {
        CalculateDtoFinancials(dto.Items);

        var invoice = _mapper.Map<Invoice>(dto);

        invoice.TotalNetAmount = invoice.Items.Sum(i => i.NetValue);
        invoice.TotalVatAmount = invoice.Items.Sum(i => i.VatAmount);
        invoice.TotalGrossAmount = invoice.Items.Sum(i => i.GrossValue);

        invoice.InvoiceNumber = await GetNextInvoiceNumberAsync(invoice.IssueDate, InvoiceType.Original);

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
    {
        var query = _dbContext.Invoices
            .Include(i => i.Items)
            .Include(i => i.Order)
            .Include(i => i.OriginalInvoice)
            .ThenInclude(oi => oi!.Items)
            .AsNoTracking();

        query = query.OrderByDescending(o => o.IssueDate.Year)
                        .ThenByDescending(o => o.IssueDate.Month)
                        .ThenByDescending(o => o.InvoiceNumber);


        var invoices = await query.ToListAsync();

        return _mapper.Map<List<InvoiceDto>>(invoices);
    }

    public async Task<InvoiceDto> GetByIdAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .Include(i => i.OriginalInvoice)
            .ThenInclude(oi => oi!.Items)
          .FirstOrDefaultAsync(o => o.Id == id);

        return invoice == null ? throw new KeyNotFoundException("Nie znaleziono faktury.") : _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task UpdateAsync(InvoiceDto dto)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .SingleOrDefaultAsync(o => o.Id == dto.Id) ?? throw new KeyNotFoundException("Nie znaleziono faktury do aktualizacji.");
        _mapper.Map(dto, invoice);

        invoice.TotalNetAmount = invoice.Items.Sum(i => i.NetValue);
        invoice.TotalVatAmount = invoice.Items.Sum(i => i.VatAmount);
        invoice.TotalGrossAmount = invoice.Items.Sum(i => i.GrossValue);

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(o => o.Id == id) ?? throw new KeyNotFoundException("Faktura nie istnieje.");
        _dbContext.Invoices.Remove(invoice);
        await _dbContext.SaveChangesAsync();

    }
    public async Task<byte[]> GenerateInvoicePdf(Invoice invoice)
    {
        var isCorrection = invoice.Type == InvoiceType.Correction;

        var model = new InvoiceViewModel
        {
            IsCorrection = isCorrection,
            Currency = invoice.Currency,

            DocumentTitle = isCorrection ? "FAKTURA VAT KORYGUJĄCA" : "FAKTURA VAT",
            DocumentNumber = $"Nr {invoice.InvoiceNumber}/{invoice.IssueDate:MM}/{invoice.IssueDate:yyyy}" + (isCorrection ? "K" : ""),

            Seller = new PartyInfoViewModel
            {
                Name = _orgSettings.LegalInfo.Name,
                Address = _orgSettings.LegalInfo.Address,
                NIP = _orgSettings.LegalInfo.NIP
            },
            Buyer = new PartyInfoViewModel
            {
                Name = invoice.BuyerName,
                Address = invoice.BuyerAddress,
                NIP = invoice.BuyerNIP
            },
            Bank = new BankInfoViewModel
            {
                BankName = _orgSettings.Bank.BankName,
                AccountPLN = _orgSettings.Bank.AccountPLN,
                AccountEUR = _orgSettings.Bank.AccountEUR,
                IBAN = _orgSettings.Bank.IBAN,
                SWIFT = _orgSettings.Bank.SWIFT
            },

            PlaceOfIssue = _orgSettings.LegalInfo.PlaceOfIssue,
            SaleDate = invoice.SaleDate,
            IssueDate = invoice.IssueDate,
            PaymentMethod = invoice.PaymentMethod,
            PaymentDeadline = $"{invoice.PaymentDeadlineDays} dni",
            Comments = invoice.Comments,

            PrimaryColorLight = "#E68E8C",
            PrimaryColorDark = "#D9534F",
            SecondaryColorLight = "#D9D9D9",
            SecondaryColorDark = "#000000",
            LabelColor = "#616161",

            Items = [],
            OriginalItems = [],
            DifferenceItems = []
        };

        decimal finalNet = invoice.TotalNetAmount;
        decimal finalVat = invoice.TotalVatAmount;
        decimal finalGross = invoice.TotalGrossAmount;

        if (isCorrection && invoice.OriginalInvoice != null)
        {
            model.CorrectionReason = invoice.Comments;
            model.Comments = $"Dotyczy faktury nr {invoice.OriginalInvoice.InvoiceNumber}/{invoice.OriginalInvoice.IssueDate:MM}/{invoice.OriginalInvoice.IssueDate:yyyy}.";

            int lpOrig = 0;
            foreach (var item in invoice.OriginalInvoice.Items)
            {
                lpOrig++;
                model.OriginalItems.Add(MapToViewModel(item, lpOrig, model.CurrencySymbol));
            }

            decimal deltaNet = invoice.TotalNetAmount - invoice.OriginalInvoice.TotalNetAmount;
            decimal deltaVat = invoice.TotalVatAmount - invoice.OriginalInvoice.TotalVatAmount;
            decimal deltaGross = invoice.TotalGrossAmount - invoice.OriginalInvoice.TotalGrossAmount;

            finalNet = deltaNet;
            finalVat = deltaVat;
            finalGross = deltaGross;
        }

        int lp = 0;
        foreach (var item in invoice.Items)
        {
            lp++;
            model.Items.Add(MapToViewModel(item, lp, model.CurrencySymbol));
            model.DifferenceItems.Add(MapToViewModel(item, lp, model.CurrencySymbol));
        }

        EuroRateDto euroRate = new();
        if (model.RequiresCurrencyConversion)
        {
            euroRate = await _euroRateService.GetEuroRateAsync(invoice.SaleDate);
        }

        model.Summary = new InvoiceSummaryViewModel
        {
            TotalToPay = FormatCurrency(finalGross, model.CurrencySymbol),
            TotalNet = FormatCurrency(finalNet, model.CurrencySymbol),
            TotalVat = FormatCurrency(finalVat, model.CurrencySymbol),
            TotalGross = FormatCurrency(finalGross, model.CurrencySymbol),

            InWordsEuro = NumberToWordsConverter.AmountInWords(finalGross, model.Currency.ToString())
        };

        if (model.RequiresCurrencyConversion && euroRate != null)
        {
            decimal vatPln = Math.Round(finalVat * euroRate.Rate, 2);
            decimal totalPln = Math.Round(finalGross * euroRate.Rate, 2);

            model.ExchangeRateInfo = new CurrencyInfoViewModel
            {
                ExchangeRate = euroRate.Rate.ToString("F4", CultureInfo.GetCultureInfo("pl-PL")) + " zł",
                NbpTable = euroRate.TableNumber,
                NbpDate = euroRate.EffectiveDate.ToString("dd-MM-yyyy")
            };

            model.Summary.VatInPLN = FormatCurrency(vatPln, "zł");
            model.Summary.VatInPLNWords = NumberToWordsConverter.AmountInWords(vatPln, "PLN");

            model.Summary.AllInPLN = FormatCurrency(totalPln, "zł");
            model.Summary.AllInPLNWords = NumberToWordsConverter.AmountInWords(totalPln, "PLN");
        }
        else
        {
            model.Summary.VatInPLN = FormatCurrency(finalVat, "zł");
            model.Summary.VatInPLNWords = NumberToWordsConverter.AmountInWords(finalVat, "PLN");

            model.Summary.AllInPLN = FormatCurrency(finalGross, "zł");
            model.Summary.AllInPLNWords = NumberToWordsConverter.AmountInWords(finalGross, "PLN");
        }

        var document = new InvoiceDocument(model);
        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static InvoiceItemViewModel MapToViewModel(InvoiceItem item, int lp, string currencySymbol)
    {
        return new InvoiceItemViewModel
        {
            Number = lp.ToString(),
            Name = item.Name,
            Quantity = item.Quantity.ToString(),
            Unit = item.Unit,
            NetPrice = FormatCurrency(item.NetPrice, currencySymbol),
            NetValue = FormatCurrency(item.NetValue, currencySymbol),
            VatRate = item.VatRate == 0 ? "NP" : (item.VatRate * 100).ToString("F0") + "%",
            VatAmount = FormatCurrency(item.VatAmount, currencySymbol),
            GrossValue = FormatCurrency(item.GrossValue, currencySymbol)
        };
    }

    private static string FormatCurrency(decimal amount, string currencySymbol = "")
    {
        return string.Format(CultureInfo.GetCultureInfo("pl-PL"), "{0:N2} {1}", amount, currencySymbol).Trim();
    }
    public async Task<(byte[] Bytes, string MimeType, string OriginalName)> GetInvoiceAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.OriginalInvoice)
                .ThenInclude(oi => oi!.Items)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new KeyNotFoundException("Nie znaleziono faktury.");

        var pdfBytes = await GenerateInvoicePdf(invoice);
        var fileName = $"FAKTURA_{invoice.InvoiceNumber}-{invoice.IssueDate:MM'-'yyyy}.pdf";
        var mimeType = "application/pdf";


        return (pdfBytes, mimeType, fileName);
    }
    public async Task<InvoiceDto> CreateFromOrderAsync(Guid orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Company)
            .FirstOrDefaultAsync(o => o.Id == orderId) ?? throw new KeyNotFoundException($"Nie znaleziono zlecenia o ID: {orderId}");

        var exists = await _dbContext.Invoices.AnyAsync(i => i.OrderId == orderId);
        if (exists)
            throw new InvalidOperationException("Faktura do tego zlecenia została już wystawiona.");

        decimal vatRate = order.Company.Country switch
        {
            "PL" => 0.23m,
            _ => 0m
        };

        decimal netAmount = order.Price;
        decimal vatAmount = Math.Round(netAmount * vatRate, 2);
        decimal grossAmount = netAmount + vatAmount;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            InvoiceNumber = await GetNextInvoiceNumberAsync(DateTime.UtcNow, InvoiceType.Original),
            IssueDate = DateTime.Now,
            SaleDate = order.UnloadingDate,

            BuyerName = order.Company.Name,
            BuyerAddress = order.Company.Address,
            BuyerNIP = order.Company.NIP,

            PaymentMethod = "Przelew",
            PaymentDeadlineDays = order.PaymentDeadline,
            Currency = order.Currency,

            TotalNetAmount = netAmount,
            TotalVatAmount = vatAmount,
            TotalGrossAmount = grossAmount,

            Comments = $"Zlecenie nr {order.OrderNumber}",
            Items = []
        };

        var item = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Name = "Usługa transportowa",
            Unit = "Usługa",
            Quantity = 1,
            NetPrice = netAmount,
            NetValue = netAmount,

            VatRate = vatRate,
            VatAmount = vatAmount,
            GrossValue = grossAmount
        };
        invoice.Items.Add(item);

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(invoice);
    }
    private async Task<int> GetNextInvoiceNumberAsync(DateTime date, InvoiceType type)
    {
        var maxNumber = await _dbContext.Invoices
                .Where(i => i.IssueDate.Year == date.Year && i.IssueDate.Month == date.Month && i.Type == type)
                .MaxAsync(i => (int?)i.InvoiceNumber) ?? 0;

        return maxNumber + 1;
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

        CalculateDtoFinancials(dto.Items);

        var correction = _mapper.Map<Invoice>(original);

        correction.Id = Guid.NewGuid();
        correction.Type = InvoiceType.Correction;
        correction.OriginalInvoiceId = original.Id;
        correction.OrderId = null;
        correction.InvoiceNumber = await GetNextInvoiceNumberAsync(dto.IssueDate ?? DateTime.Now, InvoiceType.Correction);

        correction.IssueDate = dto.IssueDate ?? DateTime.Now;
        correction.Comments = dto.Comments;
        if (dto.DeliveryMethod.HasValue) correction.DeliveryMethod = dto.DeliveryMethod;
        correction.SentDate = dto.SentDate;
        correction.HasPaid = dto.HasPaid;

        CalculateAndSetDelta(correction, dto.Items);

        correction.Items = _mapper.Map<List<InvoiceItem>>(dto.Items);
        foreach (var item in correction.Items)
        {
            item.Id = Guid.NewGuid();
        }

        _dbContext.Invoices.Add(correction);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(correction);
    }

    public async Task UpdateCorrectionAsync(InvoiceCorrectionDto dto)
    {
        var correction = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == dto.Id)
            ?? throw new KeyNotFoundException("Nie znaleziono korekty.");

        CalculateDtoFinancials(dto.Items);

        correction.IssueDate = dto.IssueDate ?? DateTime.Now;
        correction.Comments = dto.Comments;
        correction.DeliveryMethod = dto.DeliveryMethod;
        correction.SentDate = dto.SentDate;
        correction.HasPaid = dto.HasPaid;
        correction.InvoiceNumber = dto.InvoiceNumber;

        CalculateAndSetDelta(correction, dto.Items);

        SyncInvoiceItems(correction, dto.Items);

        await _dbContext.SaveChangesAsync();
    }


    private static void CalculateDtoFinancials(IEnumerable<InvoiceItemDto> items)
    {
        foreach (var item in items)
        {
            item.NetValue = Math.Round(item.NetPrice * item.Quantity, 2);
            item.VatAmount = Math.Round(item.NetValue * item.VatRate, 2);
            item.GrossValue = item.NetValue + item.VatAmount;
        }
    }

    private static void CalculateAndSetDelta(Invoice correction, ICollection<InvoiceItemDto> targetItems)
    {
        decimal targetNet = targetItems.Sum(x => x.NetValue);
        decimal targetVat = targetItems.Sum(x => x.VatAmount);
        decimal targetGross = targetItems.Sum(x => x.GrossValue);

        correction.TotalNetAmount = targetNet;
        correction.TotalVatAmount = targetVat;
        correction.TotalGrossAmount = targetGross;
    }

    private void SyncInvoiceItems(Invoice correction, ICollection<InvoiceItemDto> incomingItems)
    {
        var incomingIds = incomingItems.Select(x => x.Id).ToHashSet();
        var itemsToDelete = correction.Items.Where(x => !incomingIds.Contains(x.Id)).ToList();

        if (itemsToDelete.Count != 0)
        {
            _dbContext.Set<InvoiceItem>().RemoveRange(itemsToDelete);
        }

        foreach (var dtoItem in incomingItems)
        {
            var existingItem = correction.Items.FirstOrDefault(x => x.Id == dtoItem.Id);

            if (existingItem != null)
            {
                _mapper.Map(dtoItem, existingItem);
            }
            else
            {
                var newItem = _mapper.Map<InvoiceItem>(dtoItem);

                if (newItem.Id == Guid.Empty) newItem.Id = Guid.NewGuid();
                newItem.InvoiceId = correction.Id;

                correction.Items.Add(newItem);
            }
        }
    }
}
