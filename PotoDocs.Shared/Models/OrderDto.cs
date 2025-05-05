using System.ComponentModel.DataAnnotations;

namespace PotoDocs.Shared.Models;

public class OrderDto
{
    /*
        Dane o zleceniu
    */
    public Guid? Id { get; set; }
    public int? InvoiceNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public UserDto? Driver { get; set; }

    public CompanyDto Company { get; set; }

    /*
        Dane zapłata
    */
    [Range(0, double.MaxValue, ErrorMessage = "Cena nie może być ujemna.")]
    public decimal? Price { get; set; }

    public string? Currency { get; set; }

    public int? PaymentDeadline { get; set; }
    public bool? HasPaid { get; set; }

    public List<OrderStopDto> Stops { get; set; }

    /*
        Inne
    */
    [StringLength(50, ErrorMessage = "Numer zamówienia firmy nie może mieć więcej niż 50 znaków.")]
    public string? CompanyOrderNumber { get; set; }

    /*
        Pliki
    */
    public string? PDFUrl { get; set; }

    public List<string>? CMRFiles { get; set; }
}
