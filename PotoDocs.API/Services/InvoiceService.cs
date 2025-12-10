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
}

public class InvoiceService(PotodocsDbContext dbContext, IMapper mapper, IEuroRateService euroRateService, IOptions<OrganizationSettings> options) : IInvoiceService
{
    private readonly PotodocsDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IEuroRateService _euroRateService = euroRateService;

    private readonly OrganizationSettings _orgSettings = options.Value;

    public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
    {
        var invoice = _mapper.Map<Invoice>(dto);

        _dbContext.Invoices.Add(invoice);


        await _dbContext.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(invoice);
    }
    public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
    {
        var query = _dbContext.Invoices
            .Include(i => i.Items)
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
          .FirstOrDefaultAsync(o => o.Id == id);

        return invoice == null ? throw new KeyNotFoundException("Nie znaleziono faktury.") : _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task UpdateAsync(InvoiceDto dto)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .SingleOrDefaultAsync(o => o.Id == dto.Id) ?? throw new KeyNotFoundException("Nie znaleziono faktury do aktualizacji.");
        _mapper.Map(dto, invoice);

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
        EuroRateDto euroRate = await _euroRateService.GetEuroRateAsync(invoice.SaleDate);
        decimal netAmount = invoice.Items.Sum(item => item.NetValue);
        decimal grossAmount = invoice.Items.Sum(item => item.GrossValue);
        decimal vatAmount = invoice.Items.Sum(item => item.VatAmount);
        decimal vatAmountPln = Math.Round(vatAmount * euroRate.Rate, 2);
        decimal totalAmountPln = Math.Round(grossAmount * euroRate.Rate, 2);

        var model = new InvoiceViewModel
        {
            DocumentTitle = "FAKTURA VAT",
            DocumentNumber = $"Nr {invoice.InvoiceNumber}/{invoice.IssueDate:MM}/{invoice.IssueDate:yyyy}",

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

            PlaceOfIssue = _orgSettings.LegalInfo.PlaceOfIssue,
            SaleDate = invoice.SaleDate,
            IssueDate = invoice.IssueDate,
            PaymentMethod = invoice.PaymentMethod,
            PaymentDeadline = $"{invoice.PaymentDeadlineDays} dni",

            Bank = new BankInfoViewModel
            {
                BankName = _orgSettings.Bank.BankName,
                AccountPLN = _orgSettings.Bank.AccountPLN,
                AccountEUR = _orgSettings.Bank.AccountEUR,
                IBAN = _orgSettings.Bank.IBAN,
                SWIFT = _orgSettings.Bank.SWIFT
            },

            Comments = invoice.Comments,

            Currency = new CurrencyInfoViewModel
            {
                ExchangeRate = euroRate.Rate.ToString("F4", CultureInfo.GetCultureInfo("pl-PL")) + " zł",
                NbpTable = euroRate.TableNumber,
                NbpDate = euroRate.EffectiveDate.ToString("dd-MM-yyyy")
            },
            Summary = new InvoiceSummaryViewModel
            {
                TotalToPay = FormatCurrency(grossAmount, "€"),
                TotalNet = FormatCurrency(netAmount, "€"),
                TotalVat = FormatCurrency(vatAmount, "€"),
                TotalGross = FormatCurrency(grossAmount, "€"),
                InWordsEuro = NumberToWordsConverter.AmountInWords(grossAmount, "EUR"),
                VatInPLN = FormatCurrency(vatAmountPln, "zł"),
                VatInPLNWords = NumberToWordsConverter.AmountInWords(vatAmountPln, "PLN"),
                AllInPLN = FormatCurrency(totalAmountPln, "zł"),
                AllInPLNWords = NumberToWordsConverter.AmountInWords(totalAmountPln, "PLN")
            },

            PrimaryColorLight = "#E68E8C",
            PrimaryColorDark = "#D9534F",
            LabelColor = "#616161",
            Items = []
        };
        int lp = 0;
        foreach (var item in invoice.Items)
        {
            lp++;
            model.Items.Add(new InvoiceItemViewModel
            {
                Number = lp.ToString(),
                Name = item.Name,
                Quantity = item.Quantity.ToString(),
                Unit = item.Unit,
                NetPrice = FormatCurrency(item.NetPrice, "€"),
                NetValue = FormatCurrency(item.NetValue, "€"),
                VatRate = item.VatRate == 0 ? "NP" : (item.VatRate * 100).ToString("F0") + "%",
                VatAmount = FormatCurrency(item.VatAmount, "€"),
                GrossValue = FormatCurrency(item.GrossValue, "€")
            });
        }

        var document = new InvoiceDocument(model);
        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static string FormatCurrency(decimal amount, string currencySymbol = "")
    {
        return string.Format(CultureInfo.GetCultureInfo("pl-PL"), "{0:N2} {1}", amount, currencySymbol).Trim();
    }
    public async Task<(byte[] Bytes, string MimeType, string OriginalName)> GetInvoiceAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id) ?? throw new KeyNotFoundException("Nie znaleziono faktury.");
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
            InvoiceNumber = await GetNextInvoiceNumberAsync(order.UnloadingDate),
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

    private async Task<int> GetNextInvoiceNumberAsync(DateTime date)
    {
        var maxNumber = await _dbContext.Invoices
                .Where(i => i.IssueDate.Year == date.Year && i.IssueDate.Month == date.Month)
                .MaxAsync(i => (int?)i.InvoiceNumber) ?? 0;

        return maxNumber + 1;
    }
}
