using BlazorDownloadFile;
using MudBlazor;

namespace PotoDocs.Blazor.Helpers;

public interface IFileDownloadHelper
{
    Task DownloadFromResponseAsync(HttpResponseMessage response);
    Task DownloadFromBytesAsync(string fileName, byte[] data, string contentType);
}

public class FileDownloadHelper(IBlazorDownloadFileService downloadService, ISnackbar snackbar) : IFileDownloadHelper
{
    private readonly IBlazorDownloadFileService _downloadService = downloadService;
    private readonly ISnackbar _snackbar = snackbar;

    public async Task DownloadFromResponseAsync(HttpResponseMessage response)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                _snackbar.Add($"Błąd: {response.StatusCode}", Severity.Error);
                return;
            }
            var fileName = GetFileNameFromHeaders(response);
            using var stream = await response.Content.ReadAsStreamAsync();
            await _downloadService.DownloadFile(fileName, stream, "application/octet-stream");
            _snackbar.Add($"Pobrano: {fileName}", Severity.Success);
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Błąd: {ex.Message}", Severity.Error);
        }
    }

    public async Task DownloadFromBytesAsync(string fileName, byte[] data, string contentType)
    {
        try
        {
            if (data is null || data.Length == 0)
            {
                _snackbar.Add("Plik jest pusty.", Severity.Warning);
                return;
            }
            await _downloadService.DownloadFile(fileName, data, contentType);
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Błąd zapisu: {ex.Message}", Severity.Error);
        }
    }

    private string GetFileNameFromHeaders(HttpResponseMessage response)
    {
        var headers = response.Content.Headers;
        return headers.ContentDisposition?.FileNameStar?.Trim('"')
            ?? headers.ContentDisposition?.FileName?.Trim('"')
            ?? "plik.bin";
    }
}