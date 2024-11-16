using iTextSharp.text.pdf;
using PotoDocs.API.Model;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

public interface IPdfFormFillerService
{
    Task<byte[]> FillPdfFormAsync(InvoiceDto model);
}

public class PdfFormFillerService : IPdfFormFillerService
{
    private readonly IWebHostEnvironment _env;
    private readonly string templateFileName = "template.pdf";

    public PdfFormFillerService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<byte[]> FillPdfFormAsync(InvoiceDto model)
    {
        string templatePath = Path.Combine(_env.WebRootPath, "templates", templateFileName);

        using (var pdfReader = new PdfReader(templatePath))
        using (var memoryStream = new MemoryStream())
        {
            var pdfStamper = new PdfStamper(pdfReader, memoryStream);
            var pdfFormFields = pdfStamper.AcroFields;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            BaseFont bfArialBold = BaseFont.CreateFont("c:/windows/fonts/tahomabd.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            pdfFormFields.SetField("NUMER_FAKTURY", $"Nr {model.InvoiceNumber:D2}/{model.IssueDate:MM}/{model.IssueDate:yyyy}");
            pdfFormFields.SetField("NAZWA_FIRMY", model.CompanyName);
            pdfFormFields.SetField("ADRES_FIRMY", model.CompanyAddress);
            pdfFormFields.SetField("NIP_FIRMY", model.CompanyNIP.ToString());
            pdfFormFields.SetField("DATA_SPRZEDAZY", model.SaleDate.ToString("dd-MM-yyyy"));
            pdfFormFields.SetField("DATA_WYSTAWIENIA", model.IssueDate.ToString("dd-MM-yyyy"));
            pdfFormFields.SetField("TERMIN_ZAPLATY", model.PaymentDueDate.ToString() + " dni");
            pdfFormFields.SetField("CENA_NETTO1", FormatCurrency(model.NetAmount, "€"));
            pdfFormFields.SetField("CENA_NETTO2", FormatCurrency(model.NetAmount, "€"));
            pdfFormFields.SetField("CENA_NETTO3", FormatCurrency(model.NetAmount, "€"));
            pdfFormFields.SetField("WARTOSC_BRUTTO1", FormatCurrency(model.GrossAmount, "€"));
            pdfFormFields.SetFieldProperty("WARTOSC_BRUTTO2", "textfont", bfArialBold, null);
            pdfFormFields.SetFieldProperty("WARTOSC_BRUTTO3", "textfont", bfArialBold, null);
            pdfFormFields.SetField("WARTOSC_BRUTTO2", FormatCurrency(model.GrossAmount, "€"));
            pdfFormFields.SetField("WARTOSC_BRUTTO3", FormatCurrency(model.GrossAmount, "€"));
            pdfFormFields.SetField("STAWKA_VAT", model.VATRate == 0 ? "NP" : (model.VATRate * 100).ToString("F0") + "%");
            pdfFormFields.SetField("KWOTA_VAT1", FormatCurrency(model.VATAmount, "€"));
            pdfFormFields.SetField("KWOTA_VAT2", FormatCurrency(model.VATAmount, "€"));
            pdfFormFields.SetField("SLOWNIE_EURO", model.TotalAmountInWordsEuro);
            pdfFormFields.SetField("CENA_EURO", model.EuroAmount.ToString());
            pdfFormFields.SetField("KWOTA_VAT_PLN", FormatCurrency(model.VATAmountPln));
            pdfFormFields.SetField("SLOWNIE_KWOTA_VAT_PLN", model.VATAmountInWordsPln);
            pdfFormFields.SetField("CALA_KWOTA_PLN", FormatCurrency(model.TotalAmountPln));
            pdfFormFields.SetField("SLOWNIE_CALA_KWOTA_PLN", model.TotalAmountInWordsPln);
            pdfFormFields.SetField("KURS_EURO_INFO", model.CurrencyExchangeInfo);
            pdfFormFields.SetFieldProperty("UWAGI", "textfont", bfArialBold, null);
            pdfFormFields.SetField("UWAGI", model.Remarks);

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
