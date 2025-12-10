using PotoDocs.Shared.Models;

namespace PotoDocs.API.Entities;
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string OrderNumber { get; set; } = string.Empty;
    public required decimal Price { get; set; }
    public CurrencyType Currency { get; set; } = CurrencyType.EUR;
    public int PaymentDeadline { get; set; } = 60;
    public DateTime UnloadingDate { get; set; }

    public virtual ICollection<OrderStop> Stops { get; set; } = [];
    public virtual ICollection<OrderFile> Files { get; set; } = [];

    public virtual Guid? DriverId { get; set; }
    public virtual User? Driver { get; set; }

    public required Guid CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;
    public virtual Invoice? Invoice { get; set; }
}