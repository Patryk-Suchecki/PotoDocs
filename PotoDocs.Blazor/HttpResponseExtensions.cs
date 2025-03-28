using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PotoDocs.Blazor.Helpers;

public static class HttpResponseExtensions
{
    public static async Task ThrowIfNotSuccessWithProblemDetails(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var errorText = await response.Content.ReadAsStringAsync();

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetails>(errorText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var message = problem?.Title ?? problem?.Detail ?? "Wystąpił błąd.";
            throw new Exception($"[{response.StatusCode}] {message}");
        }
        catch (JsonException)
        {
            throw new Exception($"[{response.StatusCode}] {errorText}");
        }
    }
}
