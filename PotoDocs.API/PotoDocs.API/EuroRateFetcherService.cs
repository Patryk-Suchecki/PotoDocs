using System.Text.Json;

namespace PotoDocs.API;

public static class EuroRateFetcherService
{
    public static async Task<EuroRateResult> GetEuroRateAsync(DateTime requestedDate)
    {
        const string apiUrl = "http://api.nbp.pl/api/exchangerates/rates/a/EUR/{0}/?format=json";

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        for (int i = 1; i <= 5; i++)
        {
            string formattedDate = requestedDate.AddDays(-i).ToString("yyyy-MM-dd");
            string url = string.Format(apiUrl, formattedDate);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var exchangeRateData = JsonSerializer.Deserialize<ExchangeRatesSeries>(jsonContent, options);

                var rate = exchangeRateData.Rates[0];

                return new EuroRateResult
                {
                    Rate = rate.Mid,
                    TableNumber = rate.No,
                    EffectiveDate = rate.EffectiveDate
                };
            }
        }

        throw new Exception("Nie udało się pobrać kursu EUR z ostatnich 5 dni.");
    }
}

public class EuroRateResult
{
    public decimal Rate { get; set; }
    public string TableNumber { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class ExchangeRatesSeries
{
    public string Table { get; set; }
    public string Currency { get; set; }
    public string Code { get; set; }
    public Rate[] Rates { get; set; }
}

public class Rate
{
    public string No { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal Mid { get; set; }
}
