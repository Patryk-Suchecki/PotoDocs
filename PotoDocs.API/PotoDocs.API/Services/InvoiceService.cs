using PotoDocs.Shared.Models;
using PotoDocs.API.Model;

namespace PotoDocs.API.Services;
public interface IInvoiceService
{
    Task<InvoiceDto> ConvertOpenAiToInvoice(OrderDto transportOrder);
}
public class InvoiceService : IInvoiceService
{
    IPdfFormFillerService _pdfFormFillerService;
    public InvoiceService(IPdfFormFillerService pdfFormFillerService)
    {
        _pdfFormFillerService = pdfFormFillerService;
    }
    public async Task<InvoiceDto> ConvertOpenAiToInvoice(OrderDto transportOrder)
    {
        EuroRateResult ruroRateResult = await EuroRateFetcherService.GetEuroRateAsync(transportOrder.UnloadingDate);

        string[] acceptedPolandNames = { "poland", "polska", "pl" };
        decimal vatRate = acceptedPolandNames.Contains(transportOrder.CompanyCountry.ToLowerInvariant()) ? 0.23m : 0m;

        var invoice = new InvoiceDto
        {
            InvoiceNumber = transportOrder.InvoiceNumber,
            CompanyNIP = transportOrder.CompanyNIP,
            CompanyName = transportOrder.CompanyName,
            CompanyAddress = transportOrder.CompanyAddress,
            SaleDate = transportOrder.UnloadingDate,
            IssueDate = transportOrder.InvoiceIssueDate,
            PaymentDueDate = transportOrder.PaymentDeadline,
            NetAmount = transportOrder.Price,
            Remarks = transportOrder.CompanyOrderNumber,

            // Obliczenia kwoty brutto i VAT
            GrossAmount = transportOrder.Price * (vatRate + 1), // Przykład 23% VAT
            VATRate = vatRate, // Stawka VAT 23%
            VATAmount = transportOrder.Price * vatRate,
            VATAmountPln = transportOrder.Price * vatRate * ruroRateResult.Rate, // Kwota VAT w PLN

            // Kwota w Euro na podstawie kursu
            EuroAmount = ruroRateResult.Rate,

            // Całkowite kwoty
            TotalAmountPln = transportOrder.Price * (vatRate + 1) * ruroRateResult.Rate, // Kwota brutto w PLN

            // Kwoty słownie
            TotalAmountInWordsEuro = NumberToWordsConverter.AmountInWords(transportOrder.Price * 1.23m, "EUR"),
            VATAmountInWordsPln = NumberToWordsConverter.AmountInWords(transportOrder.Price * 0.23m * ruroRateResult.Rate, "PLN"),
            TotalAmountInWordsPln = NumberToWordsConverter.AmountInWords(transportOrder.Price * 1.23m * ruroRateResult.Rate, "PLN"),

            // Informacje o kursie euro
            CurrencyExchangeInfo = ruroRateResult.Message
        };
        return invoice;
    }

    public async Task GenerateInvoicePdf(OrderDto transportOrder)
    {
        var invoiceDto = await ConvertOpenAiToInvoice(transportOrder);

        await _pdfFormFillerService.FillPdfFormAsync(invoiceDto);
    }
}
