using PotoDocs.API.Entities;

namespace PotoDocs.API.Entities;
public class Order
{
    public int Id { get; set; }
    public long CompanyNIP { get; set; }
    public string CompanyName { get; set;}
    public string CompanyAddress { get; set; }
    public string CompanyCountry { get; set; }
    public string InvoiceNumber { get; set; }
    public float PriceAmount { get; set; }
    public string PriceCurrency { get; set; }
    public int DaysToPayment { get; set; }
    public DateTime LoadingDate { get; set; }
    public string LoadingAddress { get; set; }
    public DateTime UnloadingDate { get; set; }
    public string UnloadingAddress { get; set; }
    public string Comments { get; set; }
    public DateOnly InvoiceIssueDate { get; set; }
    public bool PaymentMade { get; set; }
    public virtual User Driver { get; set; }
    public string PDFUrl { get; set; }
    public List<CMRFile> CMRFiles { get; set; }
}
