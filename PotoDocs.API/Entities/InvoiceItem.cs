namespace PotoDocs.API.Entities;
public class InvoiceItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal NetPrice { get; set; }
    public decimal NetValue { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal GrossValue { get; set; }

    public Guid InvoiceId { get; set; }
    public virtual Invoice Invoice { get; set; } = null!;
}
