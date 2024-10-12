namespace PotoDocs.API.Models;

public class AdminOrderDto
{
    public int Id { get; set; }
    public int CompanyNIP { get; set; }
    public string CompanyName { get; set; }
    public string InvoiceNumber { get; set; }
    public float Price { get; set; }
    public int DaysToPayment { get; set; }
    public DateTime LoadingDate { get; set; }
    public string LoadingAddress { get; set; }
    public DateTime UnloadingDate { get; set; }
    public DateTime UnloadingAddress { get; set; }
    public string CompanyOrderNumber { get; set; }
    public DateOnly InvoiceIssueDate { get; set; }
    public bool PaymentMade { get; set; }
    public string DriverName { get; set; }
    public string DriverLastname { get; set; }
    public string PDFUrl { get; set; }
    public List<CMRFile> CMRFiles { get; set; }
}
