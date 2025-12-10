using PotoDocs.Shared.Models;
namespace PotoDocs.API.Entities;

public class OrderStop
{
    public int Id { get; set; }
    public required StopType Type { get; set; }
    public required DateTime Date { get; set; }
    public required string Address { get; set; }

    public required Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
}