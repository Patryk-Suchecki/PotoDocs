namespace PotoDocs.API.Entities;
public class Order
{
    public int Id { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly InvoiceIssueDate { get; set; }
    public virtual User? Driver { get; set; }

    public long? CompanyNIP { get; set; }
    public string? CompanyName { get; set;}
    public string? CompanyAddress { get; set; }
    public string? CompanyCountry { get; set; }

    public float? Price { get; set; }
    public string? Currency { get; set; }
    public int? PaymentDeadline { get; set; }
    public bool? HasPaid { get; set; }

    public DateTime? LoadingDate { get; set; }
    public string? LoadingAddress { get; set; }

    public DateTime? UnloadingDate { get; set; }
    public string? UnloadingAddress { get; set; }

    public string? CompanyOrderNumber { get; set; }

    public string? PDFUrl { get; set; }
    public List<CMRFile>? CMRFiles { get; set; }
}
