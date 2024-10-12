namespace PotoDocs.API.Models;

public class CMRFile
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }

}
