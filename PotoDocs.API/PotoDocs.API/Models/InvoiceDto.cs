namespace PotoDocs.API.Model;

public class InvoiceDto
{
    public string Remarks { get; set; }
    public decimal GrossAmount { get; set; } // WARTOSC_BRUTTO#0 WARTOSC_BRUTTO#1 WARTOSC_BRUTTO#2
    public decimal NetAmount { get; set; } // CENA_NETTO#0 CENA_NETTO#1 CENA_NETTO#2
    public decimal VATRate { get; set; } // STAWKA_VAT
    public decimal VATAmount { get; set; } // KWOTA_VAT#0 KWOTA_VAT#1
    public string TotalAmountInWordsEuro { get; set; } // SLOWNIE_EURO
    public string CurrencyExchangeInfo { get; set; } // KURS_EURO_INFO
    public int InvoiceNumber { get; set; } // NUMER_FAKTURY
    public string CompanyName { get; set; } // NAZWA_FIRMY
    public string CompanyAddress { get; set; } // ADRES_FIRMY
    public long CompanyNIP { get; set; } // NIP_FIRMY
    public DateTime SaleDate { get; set; } // DATA_SPRZEDAZY
    public DateTime IssueDate { get; set; } // DATA_WYSTAWIENIA
    public int PaymentDueDate { get; set; } // TERMIN_ZAPLATY
    public decimal EuroAmount { get; set; } // CENA_EURO
    public decimal VATAmountPln { get; set; } // KWOTA_VAT_PLN
    public string VATAmountInWordsPln { get; set; } // SLOWNIE_KWOTA_VAT_PLN
    public decimal TotalAmountPln { get; set; } // CALA_KWOTA_PLN
    public string TotalAmountInWordsPln { get; set; } // SLOWNIE_CALA_KWOTA_PLN

}
