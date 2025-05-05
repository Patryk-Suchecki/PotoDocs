namespace PotoDocs.API.Entities;
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? InvoiceNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public virtual int? DriverId { get; set; }
    public virtual User? Driver { get; set; }
    public int? CompanyId { get; set; }
    public virtual Company? Company { get; set; }

    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public int? PaymentDeadline { get; set; }
    public bool? HasPaid { get; set; }

    public virtual List<OrderStop> Stops { get; set; } = new();

    public string? CompanyOrderNumber { get; set; }

    public string? PDFUrl { get; set; }
    public List<CMRFile>? CMRFiles { get; set; }
}
