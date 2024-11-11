using System.Text.Json;

namespace PotoDocs.API;

public static class EuroRateFetcherService
{
    public static async Task<EuroRateResult> GetEuroRateAsync(DateTime requestedDate)
    {
        string apiUrl = "http://api.nbp.pl/api/exchangerates/rates/a/EUR/{0}/?format=json";
        string formattedDate = requestedDate.AddDays(-1).ToString("yyyy-MM-dd");
        string url = string.Format(apiUrl, formattedDate);

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response;

            while (true)
            {
                url = string.Format(apiUrl, formattedDate);
                response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    break;
                }
                else
                {
                    requestedDate = requestedDate.AddDays(-1);
                    formattedDate = requestedDate.ToString("yyyy-MM-dd");
                }
            }

            string jsonContent = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var exchangeRateData = JsonSerializer.Deserialize<ExchangeRatesSeries>(jsonContent, options);

            var rate = exchangeRateData.Rates[0].Mid;
            var tableNo = exchangeRateData.Rates[0].No;
            var effectiveDate = exchangeRateData.Rates[0].EffectiveDate.ToString("dd.MM.yyyy");


            string message = $"Kwota VAT została przeliczona na złote polskie po kursie średnim NBP dla EUR, " +
                             $"Tabela nr. {tableNo} z {effectiveDate}.";

            return new EuroRateResult
            {
                Rate = rate,
                Message = message
            };
        }
    }

}

public class EuroRateResult
{
    public decimal Rate { get; set; }
    public string Message { get; set; }
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
