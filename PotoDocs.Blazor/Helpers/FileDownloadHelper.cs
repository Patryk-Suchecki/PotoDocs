using BlazorDownloadFile;
using MudBlazor;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Helpers;

public interface IFileDownloadHelper
{
    Task DownloadFromServerAsync(Func<Task<FileDownloadResult>> apiCall, Action<bool>? loadingStateChanged = null);
    Task DownloadFromBytesAsync(string fileName, byte[] data, string contentType);
}

public class FileDownloadHelper(IBlazorDownloadFileService downloadService, ISnackbar snackbar) : IFileDownloadHelper
{
    private readonly IBlazorDownloadFileService _downloadService = downloadService;
    private readonly ISnackbar _snackbar = snackbar;

    public async Task DownloadFromServerAsync(Func<Task<FileDownloadResult>> apiCall, Action<bool>? loadingStateChanged = null)
    {
        try
        {
            loadingStateChanged?.Invoke(true);

            var file = await apiCall();

            if (file?.FileContent is null || file.FileContent.Length == 0)
            {
                _snackbar.Add("Nie znaleziono pliku lub jest on pusty.", Severity.Info);
                return;
            }

            await _downloadService.DownloadFile(file.FileName, file.FileContent, file.ContentType);
            _snackbar.Add($"Pobrano {file.FileName}", Severity.Success);
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Błąd pobierania: {ex.Message}", Severity.Error);
        }
        finally
        {
            loadingStateChanged?.Invoke(false);
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
            _snackbar.Add($"Błąd zapisu pliku: {ex.Message}", Severity.Error);
        }
    }
}