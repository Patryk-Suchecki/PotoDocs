namespace PotoDocs.Shared.Models;

public class FileDownloadResult(Stream stream, string fileName, string contentType)
{
    public Stream FileStream { get; set; } = stream;
    public string FileName { get; set; } = fileName;
    public string ContentType { get; set; } = contentType;
}