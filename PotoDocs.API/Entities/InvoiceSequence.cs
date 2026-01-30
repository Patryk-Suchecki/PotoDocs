namespace PotoDocs.API.Entities;

public class InvoiceSequence
{
    public int Year { get; set; }
    public int Month { get; set; }
    public InvoiceType Type { get; set; }
    public int LastNumber { get; set; }
}