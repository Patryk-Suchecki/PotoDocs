using Microsoft.AspNetCore.StaticFiles;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, FileType type, Guid fileId);
    Task<(Stream Stream, string ContentType)> GetFileStreamAsync(string folderPath, string fileNameWithExt);
    Task<(Stream Stream, string ContentType)> GetFileStreamAsync(FileType type, string fileNameWithExt);
    void DeleteFile(string folderPath, string fileNameWithExt);
}

public class LocalFileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private readonly IWebHostEnvironment _env = env;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public async Task<string> SaveFileAsync(IFormFile file, FileType type, Guid fileId)
    {
        var targetFolder = GetFolderPath(type);

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{fileId}{extension}";
        var fullPath = Path.Combine(targetFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return targetFolder;
    }

    public async Task<(byte[] Bytes, string MimeType)> GetFileAsync(string folderPath, string fileNameWithExt)
    {
        var fullPath = Path.Combine(folderPath, fileNameWithExt);
        return await ReadFileInternalAsync(fullPath, fileNameWithExt);
    }

    public async Task<(byte[] Bytes, string MimeType)> GetFileAsync(FileType type, string fileNameWithExt)
    {
        var folderPath = GetFolderPath(type);
        var fullPath = Path.Combine(folderPath, fileNameWithExt);
        return await ReadFileInternalAsync(fullPath, fileNameWithExt);
    }

    public void DeleteFile(string folderPath, string fileNameWithExt)
    {
        var fullPath = Path.Combine(folderPath, fileNameWithExt);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
    public Task<(Stream Stream, string ContentType)> GetFileStreamAsync(string folderPath, string fileNameWithExt)
    {
        var fullPath = Path.Combine(folderPath, fileNameWithExt);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Plik fizyczny nie istnieje: {fullPath}");
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        if (!_contentTypeProvider.TryGetContentType(fileNameWithExt, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return Task.FromResult((Stream: (Stream)stream, ContentType: contentType));
    }
    public Task<(Stream Stream, string ContentType)> GetFileStreamAsync(FileType type, string fileNameWithExt)
    {
        var folderPath = GetFolderPath(type);
        return GetFileStreamAsync(folderPath, fileNameWithExt);
    }
    private async Task<(byte[] Bytes, string MimeType)> ReadFileInternalAsync(string fullPath, string fileName)
    {
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Plik nie istnieje: {fullPath}");

        var bytes = await File.ReadAllBytesAsync(fullPath);

        if (!_contentTypeProvider.TryGetContentType(fileName, out var mimeType))
        {
            mimeType = "application/octet-stream";
        }

        return (bytes, mimeType);
    }

    private string GetFolderPath(FileType type)
    {
        return type switch
        {
            FileType.Order => Path.Combine(_env.ContentRootPath, "PrivateFiles", "zlecenia"),
            FileType.Cmr => Path.Combine(_env.ContentRootPath, "PrivateFiles", "cmr"),
            FileType.Images => Path.Combine(_env.WebRootPath, "images"),
            FileType.EmailTemplate => Path.Combine(_env.WebRootPath, "emails"),

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Nieobsługiwany typ pliku")
        };
    }
}