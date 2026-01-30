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

    public decimal TotalNetAmount { get; private set; }
    public decimal TotalVatAmount { get; private set; }
    public decimal TotalGrossAmount { get; private set; }
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
    public void AddItem(InvoiceItem item)
    {
        item.CalculateRow();
        Items.Add(item);
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        foreach (var item in Items)
        {
            item.CalculateRow();
        }

        TotalNetAmount = Items.Sum(i => i.NetValue);
        TotalVatAmount = Items.Sum(i => i.VatAmount);
        TotalGrossAmount = Items.Sum(i => i.GrossValue);
    }
    public (decimal Net, decimal Vat, decimal Gross) CalculateCorrectionDelta()
    {
        if (Type != InvoiceType.Correction)
        {
            return (TotalNetAmount, TotalVatAmount, TotalGrossAmount);
        }

        if (OriginalInvoice == null)
        {
            throw new InvalidOperationException("Nie można obliczyć różnicy korekty, ponieważ faktura pierwotna (OriginalInvoice) nie została załadowana z bazy.");
        }

        return (
            TotalNetAmount - OriginalInvoice.TotalNetAmount,
            TotalVatAmount - OriginalInvoice.TotalVatAmount,
            TotalGrossAmount - OriginalInvoice.TotalGrossAmount
        );
    }
}