namespace PotoDocs.Shared.Models;
public class FileDownloadResult
{
    public byte[] FileContent { get; set; } = [];
    public string FileName { get; set; } = "pobrany_plik.zip";
    public string ContentType { get; set; } = "application/octet-stream";
}
