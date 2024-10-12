namespace PotoDocs.API.Models;

public class DriverOrderDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public DateTime LoadingDate { get; set; }
    public string LoadingAddress { get; set; }
    public DateTime UnloadingDate { get; set; }
    public DateTime UnloadingAddress { get; set; }
    public string DriverName { get; set; }
    public string DriverLastname { get; set; }
    public List<CMRFile> CMRFiles { get; set; }
}
