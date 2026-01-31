using Microsoft.Extensions.Options;
using PotoDocs.API.Exceptions;
using PotoDocs.API.Options;
using PotoDocs.Shared.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UglyToad.PdfPig;

namespace PotoDocs.API.Services;

public interface IOpenAIService
{
    Task<OrderDto> GetInfoFromText(IFormFile file);
}

public class OpenAIService(HttpClient httpClient, IOptions<OpenAIOptions> options) : IOpenAIService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OpenAIOptions _options = options.Value;

    private static readonly JsonSerializerOptions _jsonRequestOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _jsonResponseOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<OrderDto> GetInfoFromText(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("Plik PDF jest pusty lub niepoprawny.");

        string extractedText;
        using (var stream = file.OpenReadStream())
        {
            extractedText = ExtractTextFromPdf(stream);
        }

        if (string.IsNullOrWhiteSpace(extractedText))
            throw new BadRequestException("Nie udało się odczytać tekstu z pliku PDF (może być to skan/obraz).");


        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = _options.SystemMessage },
                new { role = "user", content = _options.PromptTemplate.Replace("{TEXT}", extractedText) }
            },
            response_format = new { type = "json_object" }
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody, _jsonRequestOptions);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"Błąd OpenAI: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrEmpty(content))
                throw new Exception("OpenAI zwróciło puste pole 'content'.");

            var cleanedJson = CleanJsonMarkdown(content);

            var orderDto = JsonSerializer.Deserialize<OrderDto>(cleanedJson, _jsonResponseOptions);
            return orderDto ?? throw new Exception("Deserializacja JSON zwróciła null.");
        }
        catch (JsonException ex)
        {
            throw new BadRequestException("Otrzymano niepoprawny format danych z OpenAI.", ex);
        }
    }

    private static string CleanJsonMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        if (text.StartsWith("```json"))
        {
            text = text["```json".Length..];
        }
        else if (text.StartsWith("```"))
        {
            text = text["```".Length..];
        }

        if (text.EndsWith("```"))
        {
            text = text[..^3];
        }

        return text.Trim();
    }

    private static string ExtractTextFromPdf(Stream pdfStream)
    {
        pdfStream.Position = 0;
        using var document = PdfDocument.Open(pdfStream);
        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords();

            var lines = words
                .GroupBy(w => (int)(w.BoundingBox.Bottom / 5))
                .OrderByDescending(g => g.Key)
                .Select(g => g.OrderBy(w => w.BoundingBox.Left));

            foreach (var line in lines)
            {
                foreach (var word in line)
                {
                    builder.Append(word.Text).Append(' ');
                }
                builder.AppendLine();
            }

            if (builder.Length > 20000)
            {
                builder.AppendLine("\n[--- TRUNCATED ---]");
                break;
            }
        }

        return builder.ToString();
    }
}