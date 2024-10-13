using System.Text.Json.Serialization;

namespace PotoDocs.Shared.Models;
public class TransportOrderDto
{
    public int Id { get; set; }
    public int InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string Comments { get; set; }
    public string Driver { get; set; }
    public long CompanyNIP { get; set; }
    public string CompanyName { get; set; }
    public string CompanyAddress { get; set; }
    public string CompanyCountry { get; set; }
    public int PaymentDeadline { get; set; }
    public Money Price { get; set; }
    public DateTime LoadingDate { get; set; }
    public Address LoadingAddress { get; set; }
    public DateTime UnloadingDate { get; set; }
    public Address UnloadingAddress { get; set; }
    public string CompanyOrderNumber { get; set; }
    public bool PaymentMade { get; set; }
    public string PDFUrl { get; set; }
    public List<string> CMRFiles { get; set; }
}
public class Address
{
    public string Location { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
public class Money
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public override string ToString() => $"{Amount:N2} {Currency}";
}

[JsonSerializable(typeof(List<TransportOrderDto>))]
internal sealed partial class TransportOrderDtoContext : JsonSerializerContext { }
