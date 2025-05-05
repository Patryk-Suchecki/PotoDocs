public class InvoiceViewModel
{
    // Tytuł i numer dokumentu
    public string DocumentTitle { get; set; }
    public string DocumentNumber { get; set; }

    // Dane sprzedawcy i nabywcy
    public PartyInfo Seller { get; set; }
    public PartyInfo Buyer { get; set; }

    // Miejsce i daty
    public string PlaceOfIssue { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime IssueDate { get; set; }

    // Informacje o płatności
    public string PaymentMethod { get; set; }
    public string PaymentDeadline { get; set; }

    // Dane bankowe
    public BankInfo Bank { get; set; }

    // Uwagi
    public string Comments { get; set; }

    // Lista pozycji na fakturze
    public List<InvoiceItem> Items { get; set; }

    // Podsumowanie końcowe
    public InvoiceSummary Summary { get; set; }

    // Informacje walutowe i przeliczenia
    public CurrencyInfo Currency { get; set; }

    // Kolory
    public string PrimaryColorLight { get; set; }
    public string PrimaryColorDark { get; set; }
    public string LabelColor { get; set; }
}

// Dane kontrahenta
public class PartyInfo
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string NIP { get; set; }
}

// Dane bankowe
public class BankInfo
{
    public string BankName { get; set; }
    public string AccountPLN { get; set; }
    public string AccountEUR { get; set; }
    public string IBAN { get; set; }
    public string SWIFT { get; set; }
}

// Pojedyncza pozycja na fakturze
public class InvoiceItem
{
    public string Number { get; set; }
    public string Name { get; set; }
    public string Quantity { get; set; }
    public string Unit { get; set; }
    public string NetPrice { get; set; }
    public string NetValue { get; set; }
    public string VatRate { get; set; }
    public string VatAmount { get; set; }
    public string GrossValue { get; set; }
}

// Podsumowanie tabeli faktury
public class InvoiceSummary
{
    public string TotalToPay { get; set; }
    public string TotalNet { get; set; }
    public string TotalVat { get; set; }
    public string TotalGross { get; set; }
    public string InWordsEuro { get; set; }
    public string VatInPLN { get; set; }
    public string VatInPLNWords { get; set; }
    public string AllInPLN { get; set; }
    public string AllInPLNWords { get; set; }
}

// Informacje o kursie walut i przeliczeniach
public class CurrencyInfo
{
    public string ExchangeRate { get; set; }
    public string NbpTable { get; set; }
    public string NbpDate { get; set; }
}