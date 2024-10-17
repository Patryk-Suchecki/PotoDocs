using System.Text;
using Newtonsoft.Json;
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
    private readonly string _openAIKey;
    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _openAIKey = configuration["OpenAIKey"];
    }
    public async Task<OrderDto> GetInfoFromText(IFormFile file)
    {
        string text;

        // Użycie pliku bez zapisywania go na serwerze - wczytanie do strumienia
        using (var stream = file.OpenReadStream())
        {
            // Wywołanie metody do ekstrakcji tekstu z pliku PDF
            text = ExtractTextFromPdf(stream);
        }
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIKey}");

        var systemMessage = new
        {
            role = "system",
            content = "You are a helpful assistant who extracts structured information from text."
        };

        var userMessage = new
        {
            role = "user",
            content = $"Proszę wyodrębnij z poniższego tekstu następujące informacje i zwróć je w formacie JSON:\n\n" +
                      "{\n" +
                      "    \"CompanyNIP\": \"nip zleceniodawcy (long to musi być numeryczne nie wysyłaj mi tego jako string)\",\n" +
                      "    \"CompanyName\": \"nazwa firmy zleceniodawcy (string)\",\n" +
                      "    \"CompanyAddress\": \"adres firmy zleceniodawcy (string)\",\n" +
                      "    \"CompanyCountry\": \"kraj z jakiego jest firma zleceniodawcy\",\n" +
                      "    \"LoadingDate\": \"data załadunku (date w formacie yyyy-MM-dd)\",\n" +
                      "    \"UnloadingDate\": \"ostatnia data rozładunku (date w formacie yyyy-MM-dd)\",\n" +
                      "    \"PaymentDeadline\": \"termin płatności w dniach (int)\",\n" +
                      "    \"Price\": \"kwota netto (decimal)\",\n" +
                      "    \"Currency\": \"waluta (np PLN, EUR)\",\n" +
                      "    \"CompanyOrderNumber\": \"numer zlecenia (np ZLECENIE PRZEWOZU NR T08747/2024, zlecenie transportowe nr 123/435/sdf3, transport order number 123-1231, ORDER FOR THE CARRIER) (string)\"\n" +
                      "}\n\n" +
                      $"Tekst do analizy: {text}"
        };

        var body = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] { systemMessage, userMessage }
        };

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions",
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            // Wydobywanie zawartości odpowiedzi asystenta
            string extractedContent = parsedResponse.choices[0].message.content;
            extractedContent = extractedContent.Replace("```json", "").Replace("```", "").Trim();

            // Deserializacja odpowiedzi na obiekt OpenAIResponseDto
            try
            {
                var openAIResponse = JsonConvert.DeserializeObject<OrderDto>(extractedContent);
                return openAIResponse;
            }
            catch
            {
                return null;
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
