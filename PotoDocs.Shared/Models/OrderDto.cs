using System.Text.Json.Serialization;

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
    public long CompanyNIP { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyCountry { get; set; }
/*
    Dane zapłata
*/
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public int PaymentDeadline { get; set; }
    public bool HasPaid { get; set; }
/*
    Dane adresu załadunku
*/
    public DateTime LoadingDate { get; set; }
    public string? LoadingAddress { get; set; }
    public double LoadingLatitude { get; set; }
    public double LoadingLongitude { get; set; }
/*
    Dane adresu rozładunku
*/
    public DateTime UnloadingDate { get; set; }
    public string? UnloadingAddress { get; set; }
    public double UnloadingLatitude { get; set; }
    public double UnloadingLongitude { get; set; }
/*
    Inne
*/
    public string? CompanyOrderNumber { get; set; }

/*
    Pliki
*/
    public string? PDFUrl { get; set; }
    public List<string>? CMRFiles { get; set; }
}

[JsonSerializable(typeof(List<OrderDto>))]
internal sealed partial class OrderDtoContext : JsonSerializerContext { }
