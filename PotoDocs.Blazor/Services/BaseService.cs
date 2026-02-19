using System.Net.Http.Json;
using PotoDocs.Blazor.Helpers;

namespace PotoDocs.Blazor.Services;

public abstract class BaseService(HttpClient httpClient)
{
    protected readonly HttpClient _httpClient = httpClient;

    protected async Task<T> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        await response.ThrowIfNotSuccessWithProblemDetails();

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return default!;

        return (await response.Content.ReadFromJsonAsync<T>())!;
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
        return (await response.Content.ReadFromJsonAsync<T>())
               ?? throw new InvalidOperationException("API zwróciło null.");
    }

    protected async Task<T> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
    {
        var response = await _httpClient.PostAsync(url, content);
        await response.ThrowIfNotSuccessWithProblemDetails();
        return (await response.Content.ReadFromJsonAsync<T>())
               ?? throw new InvalidOperationException("API zwróciło null.");
    }

    protected async Task PutAsync(string url, object? payload)
    {
        var response = await _httpClient.PutAsJsonAsync(url, payload);
        await response.ThrowIfNotSuccessWithProblemDetails();
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

    protected async Task<HttpResponseMessage> GetFileResponseAsync(string url)
    {
        var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        await response.ThrowIfNotSuccessWithProblemDetails();
        return response;
    }

    protected async Task<HttpResponseMessage> PostAndGetFileResponseAsync(string url, object payload)
    {
        var response = await _httpClient.PostAsJsonAsync(url, payload);
        await response.ThrowIfNotSuccessWithProblemDetails();
        return response;
    }
}