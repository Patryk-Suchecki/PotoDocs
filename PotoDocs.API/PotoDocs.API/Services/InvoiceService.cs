using PotoDocs.API.Entities;
using PotoDocs.API;
using System.Globalization;
using PotoDocs.Shared.Models;
using QuestPDF.Fluent;

public interface IInvoiceService
{
    Task<byte[]> GenerateInvoicePdf(Order order);
}

public class InvoiceService : IInvoiceService
{
    public async Task<byte[]> GenerateInvoicePdf(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        var lastUnloadingStop = order.Stops
            .Where(stop => stop.Type == StopType.Unloading)
            .OrderByDescending(stop => stop.Date)
            .FirstOrDefault();

        if (lastUnloadingStop == null)
            throw new InvalidOperationException("Brak rozładunku w zleceniu.");

        EuroRateResult euroRateResult = await EuroRateFetcherService.GetEuroRateAsync(lastUnloadingStop.Date);

        string[] acceptedPolandNames = { "poland", "polska", "pl" };
        decimal vatRate = acceptedPolandNames.Contains(order.Company.Country.ToLowerInvariant()) ? 0.23m : 0m;
        decimal netAmount = order.Price ?? 0;
        decimal grossAmount = Math.Round(netAmount * (1 + vatRate), 2);
        decimal vatAmount = Math.Round(netAmount * vatRate, 2);
        decimal vatAmountPln = Math.Round(vatAmount * euroRateResult.Rate, 2);
        decimal totalAmountPln = Math.Round(grossAmount * euroRateResult.Rate, 2);

        var model = new InvoiceViewModel
        {
            DocumentTitle = "FAKTURA VAT",
            DocumentNumber = $"Nr {order.InvoiceNumber}/{(order.IssueDate ?? DateTime.Now):MM/yyyy}",

            Seller = new PartyInfo
            {
                Name = "Jakub Potoniec POTO-EXPRESS Transport",
                Address = "Jana Pawła II 44, 34-600 Limanowa",
                NIP = "7372233342"
            },
            Buyer = new PartyInfo
            {
                Name = order.Company.Name,
                Address = order.Company.Address,
                NIP = order.Company.NIP.ToString()
            },

            PlaceOfIssue = "Limanowa",
            SaleDate = lastUnloadingStop.Date,
            IssueDate = order.IssueDate ?? DateTime.Now,
            PaymentMethod = "Przelew",
            PaymentDeadline = $"{order.PaymentDeadline} dni",

            Bank = new BankInfo
            {
                BankName = "mBank S.A.",
                AccountPLN = "44 1140 2004 0000 3002 8244 7469",
                AccountEUR = "73 1140 2004 0000 3512 1581 5428",
                IBAN = "PL73 1140 2004 0000 3512 1581 5428",
                SWIFT = "BREXPLPWMBK"
            },

            Comments = $"Zlecenie Transportowe nr {order.CompanyOrderNumber}",

            Items = new List<InvoiceItem>
            {
                new InvoiceItem
                {
                    Number = "1",
                    Name = "Usługa Transportowa",
                    Quantity = "1",
                    Unit = "Usługa",
                    NetPrice = FormatCurrency(netAmount, "€"),
                    NetValue = FormatCurrency(netAmount, "€"),
                    VatRate = vatRate == 0 ? "NP" : (vatRate * 100).ToString("F0") + "%",
                    VatAmount = FormatCurrency(vatAmount, "€"),
                    GrossValue = FormatCurrency(grossAmount, "€")
                }
            },

            Summary = new InvoiceSummary
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

            Currency = new CurrencyInfo
            {
                ExchangeRate = euroRateResult.Rate.ToString("F4", CultureInfo.GetCultureInfo("pl-PL")) + " zł",
                NbpTable = euroRateResult.TableNumber,
                NbpDate = euroRateResult.EffectiveDate.ToString("dd-MM-yyyy")
            },

            PrimaryColorLight = "#E68E8C",
            PrimaryColorDark = "#D9534F",
            LabelColor = "#616161"
        };

        var document = new InvoiceDocument(model);
        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private string FormatCurrency(decimal amount, string currencySymbol = "")
    {
        return string.Format(CultureInfo.GetCultureInfo("pl-PL"), "{0:N2} {1}", amount, currencySymbol).Trim();
    }
}
