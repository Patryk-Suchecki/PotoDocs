using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PotoDocs.API.Models;
using PdfPigPage = UglyToad.PdfPig.Content.Page;
using PotoDocs.API.Exceptions;
using UglyToad.PdfPig;
using PotoDocs.API.Entities;

namespace PotoDocs.API.Services;

public interface IOpenAIService
{
    Task<Order> GetInfoFromText(IFormFile file);
}

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<Order> GetInfoFromText(IFormFile file)
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
            model = "gpt-4-turbo",
            messages = new[] { systemMessage, userMessage }
        };

        var httpContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new BadRequestException($"Błąd podczas przetwarzania PDF przez OpenAI: {response.StatusCode} - {responseBody}");

        try
        {
            var json = JsonConvert.DeserializeObject<dynamic>(responseBody);
            string extractedContent = json.choices[0].message.content;

            extractedContent = extractedContent.Replace("```json", "").Replace("```", "").Trim();

            var parsed = JsonConvert.DeserializeObject<Order>(extractedContent);
            return parsed ?? throw new Exception("Deserializacja zakończona null-em.");
        }
        catch (Exception ex)
        {
            throw new BadRequestException("Nie udało się sparsować odpowiedzi z OpenAI. Upewnij się, że prompt generuje poprawny JSON.", ex);
        }
    }

    private string ExtractTextFromPdf(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);
        var builder = new StringBuilder();

        foreach (PdfPigPage page in document.GetPages())
        {
            builder.AppendLine(page.Text);
        }

        return builder.ToString();
    }
}
