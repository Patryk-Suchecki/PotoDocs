using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using PdfPigPage = UglyToad.PdfPig.Content.Page;

namespace PotoDocs.API.Services;
public interface IOpenAIService
{
    Task<OrderDto> GetInfoFromText(IFormFile file);
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

    public async Task<OrderDto> GetInfoFromText(IFormFile file)
    {
        string text;
        using (var stream = file.OpenReadStream())
        {
            text = ExtractTextFromPdf(stream);
        }

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.APIKey}");

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

        var body = new
        {
            model = "gpt-4-turbo",
            messages = new[] { systemMessage, userMessage }
        };

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions",
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
        var errorMessage = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            string extractedContent = parsedResponse.choices[0].message.content;
            extractedContent = extractedContent.Replace("```json", "").Replace("```", "").Trim();

            try
            {
                var openAIResponse = JsonConvert.DeserializeObject<OrderDto>(extractedContent);
                return openAIResponse;
            }
            catch
            {
                return new OrderDto();
            }
        }
        else
        {
            throw new Exception($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }
    }

    private string ExtractTextFromPdf(Stream pdfStream)
    {
        StringBuilder textBuilder = new StringBuilder();
        var tempFile = Path.GetTempFileName();

        using (var fileStream = File.Create(tempFile))
        {
            pdfStream.CopyTo(fileStream);
        }

        using (var document = UglyToad.PdfPig.PdfDocument.Open(tempFile))
        {
            foreach (PdfPigPage page in document.GetPages())
            {
                textBuilder.Append(page.Text);
            }
        }

        File.Delete(tempFile);
        return textBuilder.ToString();
    }
}
