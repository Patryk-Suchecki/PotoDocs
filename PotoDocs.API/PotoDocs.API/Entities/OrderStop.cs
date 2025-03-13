using PotoDocs.Shared.Models;
namespace PotoDocs.API.Entities;

public class OrderStop
{
    public int Id { get; set; }
    public StopType Type { get; set; }
    public DateTime Date { get; set; }
    public string Address { get; set; }

    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; }
}

