namespace PotoDocs.API.Entities;
public class InvoiceItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal NetPrice { get; set; }
    public decimal VatRate { get; set; }
    public decimal NetValue { get; private set; }
    public decimal VatAmount { get; private set; }
    public decimal GrossValue { get; private set; }

    public Guid InvoiceId { get; set; }
    public virtual Invoice Invoice { get; set; } = null!;
    public void CalculateRow()
    {
        NetValue = Math.Round(NetPrice * Quantity, 2);
        VatAmount = Math.Round(NetValue * VatRate, 2);
        GrossValue = NetValue + VatAmount;
    }
}
