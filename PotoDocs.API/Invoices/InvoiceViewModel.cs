namespace PotoDocs.API.Invoices;
public class InvoiceViewModel
{
    public bool IsCorrection { get; set; } = false;
    public string CorrectionReason { get; set; } = string.Empty;
    public List<InvoiceItemViewModel> OriginalItems { get; set; } = [];
    public List<InvoiceItemViewModel> DifferenceItems { get; set; } = [];
    public CurrencyType Currency { get; set; } = CurrencyType.PLN;

    public string CurrencySymbol => Currency switch
    {
        CurrencyType.PLN => "zł",
        CurrencyType.EUR => "€",
        _ => Currency.ToString()
    };

    public bool RequiresCurrencyConversion => Currency != CurrencyType.PLN;
    // Tytuł i numer dokumentu
    public string DocumentTitle { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;

    // Dane sprzedawcy i nabywcy
    public PartyInfoViewModel Seller { get; set; } = new PartyInfoViewModel();
    public PartyInfoViewModel Buyer { get; set; } = new PartyInfoViewModel();

    // Miejsce i daty
    public string PlaceOfIssue { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public DateTime IssueDate { get; set; }

    // Informacje o płatności
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentDeadline { get; set; } = string.Empty;

    // Dane bankowe
    public BankInfoViewModel Bank { get; set; } = new BankInfoViewModel();

    // Uwagi
    public string Comments { get; set; } = string.Empty;

    // Lista pozycji na fakturze
    public List<InvoiceItemViewModel> Items { get; set; } = [];

    // Podsumowanie końcowe
    public InvoiceSummaryViewModel Summary { get; set; } = new InvoiceSummaryViewModel();

    // Informacje walutowe i przeliczenia
    public CurrencyInfoViewModel ExchangeRateInfo { get; set; } = new CurrencyInfoViewModel();

    // Kolory
    public string PrimaryColorLight { get; set; } = string.Empty;
    public string PrimaryColorDark { get; set; } = string.Empty;
    public string SecondaryColorLight { get; set; } = string.Empty;
    public string SecondaryColorDark { get; set; } = string.Empty;
    public string LabelColor { get; set; } = string.Empty;
}

// Dane kontrahenta
public class PartyInfoViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string NIP { get; set; } = string.Empty;
}

// Dane bankowe
public class BankInfoViewModel
{
    public string BankName { get; set; } = string.Empty;
    public string AccountPLN { get; set; } = string.Empty;
    public string AccountEUR { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string SWIFT { get; set; } = string.Empty;
}

// Pojedyncza pozycja na fakturze
public class InvoiceItemViewModel
{
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string NetPrice { get; set; } = string.Empty;
    public string NetValue { get; set; } = string.Empty;
    public string VatRate { get; set; } = string.Empty;
    public string VatAmount { get; set; } = string.Empty;
    public string GrossValue { get; set; } = string.Empty;
}

// Podsumowanie tabeli faktury
public class InvoiceSummaryViewModel
{
    public string TotalToPay { get; set; } = string.Empty;
    public string TotalNet { get; set; } = string.Empty;
    public string TotalVat { get; set; } = string.Empty;
    public string TotalGross { get; set; } = string.Empty;
    public string InWordsEuro { get; set; } = string.Empty;
    public string VatInPLN { get; set; } = string.Empty;
    public string VatInPLNWords { get; set; } = string.Empty;
    public string AllInPLN { get; set; } = string.Empty;
    public string AllInPLNWords { get; set; } = string.Empty;
}

// Informacje o kursie walut i przeliczeniach
public class CurrencyInfoViewModel
{
    public string ExchangeRate { get; set; } = string.Empty;
    public string NbpTable { get; set; } = string.Empty;
    public string NbpDate { get; set; } = string.Empty;
}