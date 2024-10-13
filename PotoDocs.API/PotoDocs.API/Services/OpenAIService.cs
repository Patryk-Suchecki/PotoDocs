using System.Text;
using Newtonsoft.Json;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services;
public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _openAIKey;
    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _openAIKey = configuration["ApiSettings:OpenAIKey"];
    }
    public async Task<TransportOrderDto> GetInfoFromText(string text)
    {
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
                      "    \"CompanyNIP\": \"nip zleceniodawcy (string)\",\n" +
                      "    \"CompanyName\": \"nazwa firmy zleceniodawcy (string)\",\n" +
                      "    \"CompanyAddress\": \"adres firmy zleceniodawcy (string)\",\n" +
                      "    \"CompanyCountry\": \"kraj z jakiego jest firma zleceniodawcy\",\n" +
                      "    \"LoadingDate\": \"data załadunku (date w formacie yyyy-MM-dd)\",\n" +
                      "    \"UnloadingDate\": \"ostatnia data rozładunku (date w formacie yyyy-MM-dd)\",\n" +
                      "    \"PaymentDeadline\": \"termin płatności w dniach (int)\",\n" +
                      "    \"TotalAmount\": {" +
                      "        \"Amount\": \"kwota netto (decimal)\",\n" +
                      "        \"Currency\": \"waluta (np PLN, EUR)\"},\n" +
                      "    \"Comments\": \"numer zlecenia (np ZLECENIE PRZEWOZU NR T08747/2024, zlecenie transportowe nr 123/435/sdf3, transport order number 123-1231, ORDER FOR THE CARRIER) (string)\"\n" +
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
            var openAIResponse = JsonConvert.DeserializeObject<TransportOrderDto>(extractedContent);
            return openAIResponse;
        }
        else
        {
            throw new Exception($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }
    }
}
