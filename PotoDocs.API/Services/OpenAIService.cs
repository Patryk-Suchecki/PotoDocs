using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PdfPigPage = UglyToad.PdfPig.Content.Page;
using PotoDocs.API.Exceptions;
using UglyToad.PdfPig;
using PotoDocs.Shared.Models;
using PotoDocs.API.Options;

namespace PotoDocs.API.Services;

public interface IOpenAIService
{
    Task<OrderDto> GetInfoFromText(IFormFile file);
}

public class OpenAIService(HttpClient httpClient, IOptions<OpenAIOptions> options) : IOpenAIService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OpenAIOptions _options = options.Value;

    public async Task<OrderDto> GetInfoFromText(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("Plik PDF jest pusty lub niepoprawny.");

        string text;
        using (var stream = file.OpenReadStream())
        {
            text = ExtractTextFromPdf(stream);
        }

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_options.APIKey}");

        var systemMessage = new
        {
            role = "system",
            content = _options.SystemMessage
        };

        var userMessage = new
        {
            role = "user",
            content = _options.PromptTemplate.Replace("{TEXT}", text)
        };

        var requestBody = new
        {
            model = "gpt-5.1",
            messages = new[] { systemMessage, userMessage },
            response_format = new { type = "json_object" }
        };

        var httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new BadRequestException($"Błąd podczas przetwarzania PDF przez OpenAI: {response.StatusCode} - {responseBody}");

        try
        {
            var json = JsonConvert.DeserializeObject<dynamic>(responseBody);
            string extractedContent = json?.choices?[0]?.message?.content?.ToString() ?? string.Empty;
            extractedContent = extractedContent.Replace("```json", "").Replace("```", "").Trim();

            var parsed = JsonConvert.DeserializeObject<OrderDto>(extractedContent);
            return parsed ?? throw new Exception("Deserializacja zakończona null-em.");
        }
        catch (Exception ex)
        {
            throw new BadRequestException("Nie udało się sparsować odpowiedzi z OpenAI. Upewnij się, że prompt generuje poprawny JSON.", ex);
        }
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
                var lineText = string.Join(" ", line.Select(w => w.Text));
                builder.AppendLine(lineText);
            }

            builder.AppendLine("\n--- PAGE BREAK ---\n");
        }

        return builder.ToString();
    }
}