using System.Text.Json.Serialization;

namespace PotoDocs.Model;

public class TransportOrder
{
    public int InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string Driver { get; set; }
    public string CompanyNIP { get; set; }
    public string CompanyName { get; set; }
    public string CompanyAddress { get; set; }
    public string CompanyCountry { get; set; }
    public int PaymentDeadline { get; set; }
    public Money TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime LoadingDate { get; set; }
    public DateTime UnloadingDate { get; set; }
    public string Comments { get; set; }
    public string OrderNumber { get; set; }
    public string PaymentStatus { get; set; }
    public Address LoadingAddress { get; set; }
    public Address UnloadingAddress { get; set; }
    public string PdfUrl { get; set; }
    public List<string> CmrUrls { get; set; }
}
public class Address
{
    public string Location { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
public enum OrderStatus
{
    Zakonczono,
    BrakCMR,
    Niezaplacono
}
public class Money
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public override string ToString() => $"{Amount:N2} {Currency}";
}


[JsonSerializable(typeof(List<TransportOrder>))]
internal sealed partial class TransportOrderContext : JsonSerializerContext {}