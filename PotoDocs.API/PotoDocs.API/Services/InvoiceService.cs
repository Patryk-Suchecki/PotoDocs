using iTextSharp.text.pdf;
using PotoDocs.API.Entities;
using PotoDocs.API;
using System.Globalization;
public interface IInvoiceService
{
    Task<byte[]> GenerateInvoicePdf(Order order);
}
public class InvoiceService : IInvoiceService
{
    private readonly string templateFileName = "template.pdf";

    public async Task<byte[]> GenerateInvoicePdf(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        return await FillPdfFormAsync(order);
    }

    public async Task<byte[]> FillPdfFormAsync(Order order)
    {
        EuroRateResult euroRateResult = await EuroRateFetcherService.GetEuroRateAsync((DateTime)order.UnloadingDate);

        string[] acceptedPolandNames = { "poland", "polska", "pl" };
        decimal vatRate = acceptedPolandNames.Contains(order.CompanyCountry.ToLowerInvariant()) ? 0.23m : 0m;

        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates", templateFileName);

        using (var pdfReader = new PdfReader(templatePath))
        using (var memoryStream = new MemoryStream())
        {
            var pdfStamper = new PdfStamper(pdfReader, memoryStream);
            var pdf = pdfStamper.AcroFields;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            BaseFont bfArialBold = BaseFont.CreateFont("c:/windows/fonts/tahomabd.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            decimal netAmount = (decimal)order.Price;
            decimal grossAmount = netAmount * (vatRate + 1);
            decimal vatAmount = netAmount * vatRate;
            decimal vatAmountPln = vatAmount * euroRateResult.Rate;
            decimal totalAmountPln = grossAmount * euroRateResult.Rate;

            pdf.SetField("NUMER_FAKTURY", $"Nr {order.InvoiceNumber / 1000000:D2}/{order.InvoiceIssueDate:MM}/{order.InvoiceIssueDate:yyyy}");
            pdf.SetField("NAZWA_FIRMY", order.CompanyName);
            pdf.SetField("ADRES_FIRMY", order.CompanyAddress);
            pdf.SetField("NIP_FIRMY", order.CompanyNIP.ToString());
            pdf.SetField("DATA_SPRZEDAZY", ((DateTime)order.UnloadingDate).ToString("dd-MM-yyyy"));
            pdf.SetField("DATA_WYSTAWIENIA", ((DateTime)order.InvoiceIssueDate).ToString("dd-MM-yyyy"));
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
            pdf.SetField("CENA_EURO", euroRateResult.Rate.ToString());
            pdf.SetField("KWOTA_VAT_PLN", FormatCurrency(vatAmountPln, "zł"));
            pdf.SetField("SLOWNIE_KWOTA_VAT_PLN", NumberToWordsConverter.AmountInWords(vatAmountPln, "PLN"));
            pdf.SetField("CALA_KWOTA_PLN", FormatCurrency(totalAmountPln, "zł"));
            pdf.SetField("SLOWNIE_CALA_KWOTA_PLN", NumberToWordsConverter.AmountInWords(totalAmountPln, "PLN"));
            pdf.SetField("KURS_EURO_INFO", euroRateResult.Message);
            pdf.SetFieldProperty("UWAGI", "textfont", bfArialBold, null);
            pdf.SetField("UWAGI", order.CompanyOrderNumber);

            pdfStamper.FormFlattening = true;
            pdfStamper.Close();

            return memoryStream.ToArray();
        }
    }

    private string FormatCurrency(decimal amount, string currencySymbol = "")
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:N2} {1}", amount, currencySymbol).Trim();
    }
}
