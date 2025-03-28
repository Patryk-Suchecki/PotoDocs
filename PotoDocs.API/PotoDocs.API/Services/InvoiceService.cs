using iTextSharp.text.pdf;
using PotoDocs.API.Entities;
using PotoDocs.API;
using System.Globalization;
using PotoDocs.Shared.Models;
public interface IInvoiceService
{
    Task<byte[]> GenerateInvoicePdf(Order order);
}
public class InvoiceService : IInvoiceService
{
    private readonly string _templateFilePath;
    private readonly string _fontPath;

    public InvoiceService()
    {
        _templateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates", "template.pdf");
        _fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/fonts", "tahomabd.ttf");
    }

    public Task<byte[]> GenerateInvoicePdf(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        return FillPdfFormAsync(order);
    }

    private async Task<byte[]> FillPdfFormAsync(Order order)
    {
        var lastUnloadingStop = order.Stops
            .Where(stop => stop.Type == StopType.Unloading)
            .OrderByDescending(stop => stop.Date)
            .FirstOrDefault();

        if (lastUnloadingStop == null)
            throw new InvalidOperationException("Brak rozładunku w zleceniu.");

        EuroRateResult euroRateResult = await EuroRateFetcherService.GetEuroRateAsync(lastUnloadingStop.Date);

        string[] acceptedPolandNames = { "poland", "polska", "pl" };
        decimal vatRate = acceptedPolandNames.Contains(order.Company.Country.ToLowerInvariant()) ? 0.23m : 0m;

        if (!File.Exists(_templateFilePath))
            throw new FileNotFoundException("Szablon PDF nie został znaleziony.", _templateFilePath);

        if (!File.Exists(_fontPath))
            throw new FileNotFoundException("Font PDF nie został znaleziony.", _fontPath);

        using var pdfReader = new PdfReader(_templateFilePath);
        using var memoryStream = new MemoryStream();
        using var pdfStamper = new PdfStamper(pdfReader, memoryStream);

        var pdf = pdfStamper.AcroFields;

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        BaseFont bfArialBold = BaseFont.CreateFont(_fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

        decimal netAmount = (decimal)order.Price;
        decimal grossAmount = netAmount * (vatRate + 1);
        decimal vatAmount = netAmount * vatRate;
        decimal vatAmountPln = vatAmount * euroRateResult.Rate;
        decimal totalAmountPln = grossAmount * euroRateResult.Rate;

        pdf.SetField("NUMER_FAKTURY", $"Nr {order.InvoiceNumber}/{order.IssueDate:MM}/{order.IssueDate:yyyy}");
        pdf.SetField("NAZWA_FIRMY", order.Company.Name);
        pdf.SetField("ADRES_FIRMY", order.Company.Address);
        pdf.SetField("NIP_FIRMY", order.Company.NIP.ToString());
        pdf.SetField("DATA_SPRZEDAZY", lastUnloadingStop.Date.ToString("dd-MM-yyyy"));
        pdf.SetField("DATA_WYSTAWIENIA", order.IssueDate?.ToString("dd-MM-yyyy"));
        pdf.SetField("TERMIN_ZAPLATY", order.PaymentDeadline + " dni");
        pdf.SetField("CENA_NETTO1", FormatCurrency(netAmount, "€"));
        pdf.SetField("CENA_NETTO2", FormatCurrency(netAmount, "€"));
        pdf.SetField("CENA_NETTO3", FormatCurrency(netAmount, "€"));
        pdf.SetField("WARTOSC_BRUTTO1", FormatCurrency(grossAmount, "€"));
        pdf.SetFieldProperty("WARTOSC_BRUTTO2", "textfont", bfArialBold, null);
        pdf.SetFieldProperty("WARTOSC_BRUTTO3", "textfont", bfArialBold, null);
        pdf.SetField("WARTOSC_BRUTTO2", FormatCurrency(grossAmount, "€"));
        pdf.SetField("WARTOSC_BRUTTO3", FormatCurrency(grossAmount, "€"));
        pdf.SetField("STAWKA_VAT", vatRate == 0 ? "NP" : (vatRate * 100).ToString("F0") + "%");
        pdf.SetField("KWOTA_VAT1", FormatCurrency(vatAmount, "€"));
        pdf.SetField("KWOTA_VAT2", FormatCurrency(vatAmount, "€"));
        pdf.SetField("SLOWNIE_EURO", NumberToWordsConverter.AmountInWords(grossAmount, "EUR"));
        pdf.SetField("CENA_EURO", euroRateResult.Rate.ToString("F4", CultureInfo.InvariantCulture));
        pdf.SetField("KWOTA_VAT_PLN", FormatCurrency(vatAmountPln, "zł"));
        pdf.SetField("SLOWNIE_KWOTA_VAT_PLN", NumberToWordsConverter.AmountInWords(vatAmountPln, "PLN"));
        pdf.SetField("CALA_KWOTA_PLN", FormatCurrency(totalAmountPln, "zł"));
        pdf.SetField("SLOWNIE_CALA_KWOTA_PLN", NumberToWordsConverter.AmountInWords(totalAmountPln, "PLN"));
        pdf.SetField("KURS_EURO_INFO", euroRateResult.Message);
        pdf.SetFieldProperty("UWAGI", "textfont", bfArialBold, null);
        pdf.SetField("UWAGI", order.CompanyOrderNumber);

        pdfStamper.FormFlattening = true;

        return memoryStream.ToArray();
    }

    private string FormatCurrency(decimal amount, string currencySymbol = "")
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:N2} {1}", amount, currencySymbol).Trim();
    }
}
