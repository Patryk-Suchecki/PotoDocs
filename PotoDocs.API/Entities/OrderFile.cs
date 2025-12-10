using PotoDocs.Shared.Models;

namespace PotoDocs.API.Entities;

public class OrderFile
{
    public Guid Id { get; set; }
    public FileType Type { get; set; }
    public required string Name { get; set; }
    public required long Size { get; set; }
    public required string Path { get; set; }
    public required string Extension { get; set; }
    public required string MimeType { get; set; }
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
}