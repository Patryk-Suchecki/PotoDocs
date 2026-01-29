namespace PotoDocs.API.Invoices;

public class InvoiceViewModel
{
    public string LogoPath { get; set; } = string.Empty;
    public bool IsCorrection { get; set; }
    public string CorrectionReason { get; set; } = string.Empty;
    public CurrencyType Currency { get; set; } = CurrencyType.PLN;

    public string CurrencySymbol => Currency switch
    {
        CurrencyType.PLN => "zł",
        CurrencyType.EUR => "€",
        _ => Currency.ToString()
    };

    public bool RequiresCurrencyConversion => Currency != CurrencyType.PLN;

    public string DocumentTitle { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;

    public string PlaceOfIssue { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public DateTime IssueDate { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentDeadline { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;

    public PartyInfoViewModel Seller { get; set; } = new();
    public PartyInfoViewModel Buyer { get; set; } = new();
    public BankInfoViewModel Bank { get; set; } = new();

    public List<InvoiceItemViewModel> Items { get; set; } = [];
    public List<InvoiceItemViewModel> OriginalItems { get; set; } = [];
    public List<InvoiceItemViewModel> DifferenceItems { get; set; } = [];

    public InvoiceSummaryViewModel Summary { get; set; } = new();
    public CurrencyInfoViewModel ExchangeRateInfo { get; set; } = new();
}

public class InvoiceItemViewModel
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;

    public decimal NetPrice { get; set; }
    public decimal NetValue { get; set; }

    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }

    public decimal GrossValue { get; set; }
}

public class InvoiceSummaryViewModel
{
    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalToPay { get; set; }

    public string InWordsEuro { get; set; } = string.Empty;

    public decimal? VatInPLN { get; set; }
    public string VatInPLNWords { get; set; } = string.Empty;

    public decimal? AllInPLN { get; set; }
    public string AllInPLNWords { get; set; } = string.Empty;
}

public class CurrencyInfoViewModel
{
    public decimal ExchangeRate { get; set; }
    public string NbpTable { get; set; } = string.Empty;
    public string NbpDate { get; set; } = string.Empty;
}

public class PartyInfoViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string NIP { get; set; } = string.Empty;
}

public class BankInfoViewModel
{
    public string BankName { get; set; } = string.Empty;
    public string AccountPLN { get; set; } = string.Empty;
    public string AccountEUR { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string SWIFT { get; set; } = string.Empty;
}