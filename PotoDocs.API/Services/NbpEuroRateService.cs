using Microsoft.Extensions.Caching.Memory;
using PotoDocs.Shared.Models;
using System.Text.Json.Serialization;

namespace PotoDocs.API.Services;

public interface IEuroRateService
{
    Task<EuroRateDto> GetEuroRateAsync(DateTime requestedDate);
}

public class NbpEuroRateService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache) : IEuroRateService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IMemoryCache _cache = cache;

    private readonly string _apiUrl = configuration["EuroRateUrlTemplate"]
        ?? "http://api.nbp.pl/api/exchangerates/rates/a/eur/{0}/?format=json";

    public async Task<EuroRateDto> GetEuroRateAsync(DateTime requestedDate)
    {
        string cacheKey = $"NBP_EUR_{requestedDate:yyyyMMdd}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);

            return await FetchRateFromNbpAsync(requestedDate);
        }) ?? throw new InvalidOperationException("Nie udało się pobrać kursu walut.");
    }

    private async Task<EuroRateDto> FetchRateFromNbpAsync(DateTime requestedDate)
    {
        for (int i = 1; i <= 10; i++)
        {
            var dateToCheck = requestedDate.AddDays(-i);
            var formattedDate = dateToCheck.ToString("yyyy-MM-dd");
            var url = string.Format(_apiUrl, formattedDate);

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<NbpTableDto>();

                if (data?.Rates is [var rate, ..])
                {
                    return new EuroRateDto
                    {
                        Rate = rate.Mid,
                        TableNumber = rate.No,
                        EffectiveDate = rate.EffectiveDate
                    };
                }
            }
        }

        throw new InvalidOperationException($"Nie udało się znaleźć tabeli kursowej NBP dla daty {requestedDate:yyyy-MM-dd} (sprawdzono 10 dni wstecz).");
    }

    private record NbpTableDto(
        [property: JsonPropertyName("rates")] NbpRateDto[] Rates
    );

    private record NbpRateDto(
        [property: JsonPropertyName("no")] string No,
        [property: JsonPropertyName("effectiveDate")] DateTime EffectiveDate,
        [property: JsonPropertyName("mid")] decimal Mid
    );
}