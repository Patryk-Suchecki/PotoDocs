using PotoDocs.Blazor.Helpers;
using PotoDocs.Shared.Models;
using System.Net.Http.Json;

namespace PotoDocs.Blazor.Services;

public abstract class BaseService(IAuthService authService)
{
    private readonly IAuthService _authService = authService;

    private static string ExtractFileNameFromResponse(HttpResponseMessage response)
    {
        var contentDisposition = response.Content.Headers.ContentDisposition;

        if (contentDisposition != null)
        {
            var fileName = contentDisposition.FileNameStar ?? contentDisposition.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return fileName.Trim('"');
            }
        }

        return $"pobrany_plik_{DateTime.Now:yyyyMMdd_HHmm}.bin";
    }


    protected async Task<T> GetAsync<T>(string url)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.GetAsync(url);

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź (null), a oczekiwano danych.");
    }

    protected async Task<FileDownloadResult> GetFileAsync(string url)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.GetAsync(url);

        await response.ThrowIfNotSuccessWithProblemDetails();

        return new FileDownloadResult
        {
            FileContent = await response.Content.ReadAsByteArrayAsync(),
            FileName = ExtractFileNameFromResponse(response),
            ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
        };
    }

    protected async Task PostAsync(string url, object? payload)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.PostAsJsonAsync(url, payload);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    protected async Task<T> PostAsync<T>(string url, object? payload)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.PostAsJsonAsync(url, payload);

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź (null) po operacji POST.");
    }

    protected async Task<FileDownloadResult> PostAndDownloadFileAsync(string url, object payload)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.PostAsJsonAsync(url, payload);

        await response.ThrowIfNotSuccessWithProblemDetails();

        return new FileDownloadResult
        {
            FileContent = await response.Content.ReadAsByteArrayAsync(),
            FileName = ExtractFileNameFromResponse(response),
            ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
        };
    }

    protected async Task<T> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.PostAsync(url, content);

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź (null) po wysłaniu plików.");
    }

    protected async Task PutAsync(string url, object? payload)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.PutAsJsonAsync(url, payload);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    protected async Task<T> PutAsync<T>(string url, object? payload)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.PutAsJsonAsync(url, payload);

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź (null) po operacji PUT.");
    }

    protected async Task PutMultipartAsync(string url, MultipartFormDataContent content)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.PutAsync(url, content);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    protected async Task DeleteAsync(string url)
    {
        var client = await _authService.GetAuthenticatedHttpClientAsync();
        var response = await client.DeleteAsync(url);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }
}