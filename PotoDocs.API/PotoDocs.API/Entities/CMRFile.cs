 using PotoDocs.API.Entities;

namespace PotoDocs.API.Entities;
public class CMRFile
{
    public int Id { get; set; }
    public string Url { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; }

}
