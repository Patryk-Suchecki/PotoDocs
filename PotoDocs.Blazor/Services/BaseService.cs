using System.Net.Http.Json;
using PotoDocs.Blazor.Helpers;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public abstract class BaseService(HttpClient httpClient)
{
    protected readonly HttpClient _httpClient = httpClient;

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
        var response = await _httpClient.GetAsync(url);

        await response.ThrowIfNotSuccessWithProblemDetails();

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default!;
        }

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result!;
    }

    protected async Task<FileDownloadResult> GetFileAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);

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
        var response = await _httpClient.PostAsJsonAsync(url, payload);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    protected async Task<T> PostAsync<T>(string url, object? payload)
    {
        var response = await _httpClient.PostAsJsonAsync(url, payload);

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź (null) po operacji POST.");
    }

    protected async Task<FileDownloadResult> PostAndDownloadFileAsync(string url, object payload)
    {
        var response = await _httpClient.PostAsJsonAsync(url, payload);

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
        var response = await _httpClient.PostAsync(url, content);

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź (null) po wysłaniu plików.");
    }

    protected async Task PutAsync(string url, object? payload)
    {
        var response = await _httpClient.PutAsJsonAsync(url, payload);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    protected async Task<T> PutAsync<T>(string url, object? payload)
    {
        var response = await _httpClient.PutAsJsonAsync(url, payload);

        await response.ThrowIfNotSuccessWithProblemDetails();

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("API zwróciło pustą odpowiedź (null) po operacji PUT.");
    }

    protected async Task PutMultipartAsync(string url, MultipartFormDataContent content)
    {
        var response = await _httpClient.PutAsync(url, content);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }

    protected async Task DeleteAsync(string url)
    {
        var response = await _httpClient.DeleteAsync(url);
        await response.ThrowIfNotSuccessWithProblemDetails();
    }
}