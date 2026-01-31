using Microsoft.Extensions.Options;
using PotoDocs.API.Entities;
using PotoDocs.API.Invoices; // Tu jest Twój InvoiceDocument (QuestPDF)
using PotoDocs.API.Options;
using PotoDocs.Shared.Models;
using PotoDocs.Shared.Utils;
using QuestPDF.Fluent;

namespace PotoDocs.API.Services;

public interface IInvoicePdfGenerator
{
    Task<byte[]> GenerateAsync(Invoice invoice, EuroRateDto? euroRate);
}

public class InvoicePdfGenerator(IOptions<OrganizationSettings> options, IFileStorageService fileStorage) : IInvoicePdfGenerator
{
    private readonly OrganizationSettings _orgSettings = options.Value;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<byte[]> GenerateAsync(Invoice invoice, EuroRateDto? euroRate)
    {
        var model = await CreateViewModelAsync(invoice, euroRate);
        var document = new InvoiceDocument(model);
        return document.GeneratePdf();
    }

    private async Task<InvoiceViewModel> CreateViewModelAsync(Invoice src, EuroRateDto? euroRate)
    {
        var model = InitializeBaseModel(src);

        var (bytes, _) = await _fileStorage.GetFileAsync(FileType.Images, "logo.png");
        model.LogoImage = bytes;

        ProcessItems(model, src);

        var financials = CalculateFinancials(model, src);

        var (Summary, ExchangeInfo) = BuildSummary(financials, model.Currency, euroRate);
        model.Summary = Summary;
        model.ExchangeRateInfo = ExchangeInfo;

        return model;
    }

    private InvoiceViewModel InitializeBaseModel(Invoice src)
    {
        return new InvoiceViewModel
        {
            IsCorrection = src.Type == InvoiceType.Correction,
            Currency = src.Currency,

            DocumentTitle = src.Type == InvoiceType.Correction ? "FAKTURA VAT KORYGUJĄCA" : "FAKTURA VAT",
            DocumentNumber = $"Nr {src.InvoiceNumber}/{src.IssueDate:MM}/{src.IssueDate:yyyy}" + (src.Type == InvoiceType.Correction ? "K" : ""),

            PlaceOfIssue = _orgSettings.LegalInfo.PlaceOfIssue,
            SaleDate = src.SaleDate,
            IssueDate = src.IssueDate,
            PaymentMethod = src.PaymentMethod,
            PaymentDeadline = $"{src.PaymentDeadlineDays} dni",
            Comments = src.Comments,

            Seller = new PartyInfoViewModel
            {
                Name = _orgSettings.LegalInfo.Name,
                Address = _orgSettings.LegalInfo.Address,
                NIP = _orgSettings.LegalInfo.NIP
            },
            Buyer = new PartyInfoViewModel
            {
                Name = src.BuyerName,
                Address = src.BuyerAddress,
                NIP = src.BuyerNIP
            },
            Bank = new BankInfoViewModel
            {
                BankName = _orgSettings.Bank.BankName,
                AccountPLN = _orgSettings.Bank.AccountPLN,
                AccountEUR = _orgSettings.Bank.AccountEUR,
                IBAN = _orgSettings.Bank.IBAN,
                SWIFT = _orgSettings.Bank.SWIFT
            }
        };
    }

    private static void ProcessItems(InvoiceViewModel model, Invoice src)
    {
        int lp = 0;
        foreach (var item in src.Items)
        {
            model.Items.Add(MapItem(item, ++lp));
            if (model.IsCorrection)
            {
                model.DifferenceItems.Add(MapItem(item, lp));
            }
        }
    }

    private static (decimal Net, decimal Vat, decimal Gross) CalculateFinancials(InvoiceViewModel model, Invoice src)
    {
        decimal finalNet = src.TotalNetAmount;
        decimal finalVat = src.TotalVatAmount;
        decimal finalGross = src.TotalGrossAmount;

        if (model.IsCorrection && src.OriginalInvoice != null)
        {
            model.CorrectionReason = src.Comments;
            model.Comments = $"Dotyczy faktury nr {src.OriginalInvoice.InvoiceNumber}/{src.OriginalInvoice.IssueDate:MM}/{src.OriginalInvoice.IssueDate:yyyy}.";

            int lpOrig = 0;
            foreach (var item in src.OriginalInvoice.Items)
            {
                model.OriginalItems.Add(MapItem(item, ++lpOrig));
            }

            var (Net, Vat, Gross) = src.CalculateCorrectionDelta();
            finalNet = Net;
            finalVat = Vat;
            finalGross = Gross;
        }

        return (finalNet, finalVat, finalGross);
    }

    private static (InvoiceSummaryViewModel Summary, CurrencyInfoViewModel ExchangeInfo) BuildSummary((decimal Net, decimal Vat, decimal Gross) financials, CurrencyType currency, EuroRateDto? rate)
    {
        var exchangeInfo = new CurrencyInfoViewModel();

        var summary = new InvoiceSummaryViewModel
        {
            TotalNet = financials.Net,
            TotalVat = financials.Vat,
            TotalGross = financials.Gross,
            TotalToPay = financials.Gross,
            InWordsEuro = NumberToWordsConverter.AmountInWords(financials.Gross, currency.ToString())
        };

        if (currency != CurrencyType.PLN && rate != null)
        {
            decimal vatPln = Math.Round(financials.Vat * rate.Rate, 2);
            decimal totalPln = Math.Round(financials.Gross * rate.Rate, 2);

            exchangeInfo = new CurrencyInfoViewModel
            {
                ExchangeRate = rate.Rate,
                NbpTable = rate.TableNumber,
                NbpDate = rate.EffectiveDate.ToString("dd-MM-yyyy")
            };

            summary.VatInPLN = vatPln;
            summary.VatInPLNWords = NumberToWordsConverter.AmountInWords(vatPln, "PLN");
            summary.AllInPLN = totalPln;
            summary.AllInPLNWords = NumberToWordsConverter.AmountInWords(totalPln, "PLN");
        }
        else
        {
            summary.VatInPLN = financials.Vat;
            summary.VatInPLNWords = NumberToWordsConverter.AmountInWords(financials.Vat, "PLN");
            summary.AllInPLN = financials.Gross;
            summary.AllInPLNWords = NumberToWordsConverter.AmountInWords(financials.Gross, "PLN");
        }

        return (summary, exchangeInfo);
    }

    private static InvoiceItemViewModel MapItem(InvoiceItem item, int lp)
    {
        return new InvoiceItemViewModel
        {
            Number = lp,
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            NetPrice = item.NetPrice,
            NetValue = item.NetValue,
            VatRate = item.VatRate,
            VatAmount = item.VatAmount,
            GrossValue = item.GrossValue
        };
    }
}