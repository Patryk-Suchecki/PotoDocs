namespace PotoDocs.Shared.Models;

public class EuroRateDto
{
    public decimal Rate { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
}
