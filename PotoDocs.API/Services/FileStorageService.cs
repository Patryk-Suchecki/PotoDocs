using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, FileType type, Guid fileId);
    Task<(byte[] Bytes, string MimeType)> GetFileAsync(string folderPath, string fileNameWithExt);
    void DeleteFile(string folderPath, string fileNameWithExt);
}

public class LocalFileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private readonly string _zleceniaPath = Path.Combine(env.ContentRootPath, "PrivateFiles", "zlecenia");
    private readonly string _cmrPath = Path.Combine(env.ContentRootPath, "PrivateFiles", "cmr");

    public async Task<string> SaveFileAsync(IFormFile file, FileType type, Guid fileId)
    {
        var targetFolder = type == FileType.Order ? _zleceniaPath : _cmrPath;
        Directory.CreateDirectory(targetFolder);

        var extension = Path.GetExtension(file.FileName);
        var fullPath = Path.Combine(targetFolder, $"{fileId}{extension}");

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return targetFolder;
    }

    public async Task<(byte[] Bytes, string MimeType)> GetFileAsync(string folderPath, string fileNameWithExt)
    {
        var fullPath = Path.Combine(folderPath, fileNameWithExt);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Plik nie istnieje na dysku.");

        var bytes = await File.ReadAllBytesAsync(fullPath);
        var mime = "application/octet-stream";
        return (bytes, mime);
    }

    public void DeleteFile(string folderPath, string fileNameWithExt)
    {
        var fullPath = Path.Combine(folderPath, fileNameWithExt);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}