using System.ComponentModel.DataAnnotations;

namespace PotoDocs.Shared.Models;

public class OrderDto
{
    /*
        Dane o zleceniu
    */
    public int InvoiceNumber { get; set; }
    public DateTime InvoiceIssueDate { get; set; }
    public UserDto? Driver { get; set; }

    /*
        Dane o zleceniodawcy
    */
    [StringLength(15, ErrorMessage = "NIP firmy nie może mieć więcej niż 15 znaków.")]
    public string? CompanyNIP { get; set; }

    [StringLength(100, ErrorMessage = "Nazwa firmy nie może mieć więcej niż 100 znaków.")]
    public string? CompanyName { get; set; }

    [StringLength(200, ErrorMessage = "Adres firmy nie może mieć więcej niż 200 znaków.")]
    public string? CompanyAddress { get; set; }

    [StringLength(50, ErrorMessage = "Kraj firmy nie może mieć więcej niż 50 znaków.")]
    public string? CompanyCountry { get; set; }

    /*
        Dane zapłata
    */
    [Range(0, double.MaxValue, ErrorMessage = "Cena nie może być ujemna.")]
    public decimal? Price { get; set; }

    [StringLength(3, ErrorMessage = "Kod waluty musi mieć maksymalnie 3 znaki.")]
    public string? Currency { get; set; }

    public int? PaymentDeadline { get; set; }
    public bool? HasPaid { get; set; }

    /*
        Dane adresu załadunku
    */
    public DateTime? LoadingDate { get; set; }

    [StringLength(200, ErrorMessage = "Adres załadunku nie może mieć więcej niż 200 znaków.")]
    public string? LoadingAddress { get; set; }

    /*
        Dane adresu rozładunku
    */
    public DateTime? UnloadingDate { get; set; }

    [StringLength(200, ErrorMessage = "Adres rozładunku nie może mieć więcej niż 200 znaków.")]
    public string? UnloadingAddress { get; set; }

    /*
        Inne
    */
    [StringLength(50, ErrorMessage = "Numer zamówienia firmy nie może mieć więcej niż 50 znaków.")]
    public string? CompanyOrderNumber { get; set; }

    /*
        Pliki
    */
    [StringLength(300, ErrorMessage = "Adres URL pliku PDF nie może mieć więcej niż 300 znaków.")]
    public string PDFUrl { get; set; }

    public List<string>? CMRFiles { get; set; }
}
