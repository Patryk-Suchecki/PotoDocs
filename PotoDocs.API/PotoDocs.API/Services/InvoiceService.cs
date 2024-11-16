using PotoDocs.API.Entities;
using PotoDocs.API.Model;
using PotoDocs.API;

public interface IInvoiceService
{
    Task<byte[]> GenerateInvoicePdf(Order order);
}

public class InvoiceService : IInvoiceService
{
    private readonly IPdfFormFillerService _pdfFormFillerService;

    public InvoiceService(IPdfFormFillerService pdfFormFillerService)
    {
        _pdfFormFillerService = pdfFormFillerService;
    }

    private async Task<InvoiceDto> ConvertOpenAiToInvoice(Order order)
    {
        EuroRateResult ruroRateResult = await EuroRateFetcherService.GetEuroRateAsync((DateTime)order.UnloadingDate);

        string[] acceptedPolandNames = { "poland", "polska", "pl" };
        decimal vatRate = acceptedPolandNames.Contains(order.CompanyCountry.ToLowerInvariant()) ? 0.23m : 0m;

        return new InvoiceDto
        {
            InvoiceNumber = (int)(order.InvoiceNumber / 1000000),
            CompanyNIP = (long)order.CompanyNIP,
            CompanyName = order.CompanyName,
            CompanyAddress = order.CompanyAddress,
            SaleDate = (DateTime)order.UnloadingDate,
            IssueDate = (DateTime)order.InvoiceIssueDate,
            PaymentDueDate = (int)order.PaymentDeadline,
            NetAmount = (decimal)order.Price,
            Remarks = order.CompanyOrderNumber,
            GrossAmount = (decimal)(order.Price * (vatRate + 1)),
            VATRate = vatRate,
            VATAmount = (decimal)(order.Price * vatRate),
            VATAmountPln = (decimal)(order.Price * vatRate * ruroRateResult.Rate),
            EuroAmount = ruroRateResult.Rate,
            TotalAmountPln = (decimal)(order.Price * (vatRate + 1) * ruroRateResult.Rate),
            TotalAmountInWordsEuro = NumberToWordsConverter.AmountInWords((decimal)(order.Price * 1.23m), "EUR"),
            VATAmountInWordsPln = NumberToWordsConverter.AmountInWords((decimal)(order.Price * 0.23m * ruroRateResult.Rate), "PLN"),
            TotalAmountInWordsPln = NumberToWordsConverter.AmountInWords((decimal)(order.Price * 1.23m * ruroRateResult.Rate), "PLN"),
            CurrencyExchangeInfo = ruroRateResult.Message
        };
    }

    public async Task<byte[]> GenerateInvoicePdf(Order order)
    {
        var invoiceDto = await ConvertOpenAiToInvoice(order);
        return await _pdfFormFillerService.FillPdfFormAsync(invoiceDto);
    }
}
