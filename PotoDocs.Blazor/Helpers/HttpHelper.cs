using PotoDocs.Shared.Models;
using System.Text.Json;

namespace PotoDocs.Blazor.Helpers;

public static class HttpHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task ThrowIfNotSuccessWithProblemDetails(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var errorText = await response.Content.ReadAsStringAsync();

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetailsDto>(errorText, JsonOptions);
            var message = problem?.Detail ?? problem?.Title ?? "Wystąpił nieoczekiwany błąd.";

            throw new Exception(message);
        }
        catch (JsonException)
        {
            var defaultMessage = $"Wystąpił błąd: {response.ReasonPhrase} (Status: {(int)response.StatusCode})";
            throw new Exception(defaultMessage);
        }
    }
}