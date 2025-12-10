namespace PotoDocs.Shared.Models;

public enum FileType
{
    Order,
    Cmr
}
public class OrderFileDto
{
    public Guid Id { get; set; }
    public FileType Type { get; set; }
    public required string Name { get; set; }
    public long Size { get; set; }
}