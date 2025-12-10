using PotoDocs.Shared.Models;
using System.Text.Json;

namespace PotoDocs.API.Services;

public interface IEuroRateService
{
    Task<EuroRateDto> GetEuroRateAsync(DateTime requestedDate);
}

public class NbpEuroRateService(HttpClient httpClient, IConfiguration configuration) : IEuroRateService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiUrl = configuration["EuroRateUrlTemplate"] ?? throw new InvalidOperationException("Nie skonfigurowano 'EuroRateUrlTemplate' w appsettings.json.");
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EuroRateDto> GetEuroRateAsync(DateTime requestedDate)
    {

        for (int i = 1; i <= 5; i++)
        {
            string formattedDate = requestedDate.AddDays(-i).ToString("yyyy-MM-dd");
            string url = string.Format(_apiUrl, formattedDate);

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var exchangeRateData = JsonSerializer.Deserialize<ExchangeRatesSeries>(jsonContent, _jsonOptions);

                if (exchangeRateData?.Rates?.Length > 0)
                {
                    var rate = exchangeRateData.Rates[0];
                    return new EuroRateDto
                    {
                        Rate = rate.Mid,
                        TableNumber = rate.No,
                        EffectiveDate = rate.EffectiveDate
                    };
                }
            }
        }

        throw new Exception("Nie udało się pobrać kursu EUR z ostatnich 5 dni.");
    }

    private class ExchangeRatesSeries
    {
        public string Table { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Rate[] Rates { get; set; } = [];
    }

    private class Rate
    {
        public string No { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public decimal Mid { get; set; }
    }
}