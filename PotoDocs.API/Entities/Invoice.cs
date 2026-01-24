namespace PotoDocs.API.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public required int InvoiceNumber { get; set; }
    public InvoiceType Type { get; set; } = InvoiceType.Original;
    public DateTime IssueDate { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime? SentDate { get; set; }
    public DeliveryMethodType? DeliveryMethod { get; set; }
    public bool HasPaid { get; set; } = false;

    public required string BuyerName { get; set; }
    public required string BuyerAddress { get; set; }
    public required string BuyerNIP { get; set; }

    public decimal TotalNetAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public decimal TotalGrossAmount { get; set; }
    public CurrencyType Currency { get; set; } = CurrencyType.EUR;

    public string PaymentMethod { get; set; } = "Przelew";
    public int PaymentDeadlineDays { get; set; } = 60;
    public string Comments { get; set; } = string.Empty;


    public virtual ICollection<InvoiceItem> Items { get; set; } = [];
    public Guid? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    public Guid? OriginalInvoiceId { get; set; }
    public virtual Invoice? OriginalInvoice { get; set; }

    public virtual ICollection<Invoice> Corrections { get; set; } = [];
}