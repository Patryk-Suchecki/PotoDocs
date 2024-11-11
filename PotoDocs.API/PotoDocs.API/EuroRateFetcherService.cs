using System.Xml;
using System.Globalization;

namespace PotoDocs.API;

public static class EuroRateFetcherService
{
    public static async Task<EuroRateResult> GetEuroRateAsync(DateTime requestedDate)
    {
        string apiUrl = $"http://api.nbp.pl/api/exchangerates/rates/a/EUR/{{0}}/?format=xml";
        string formattedDate = requestedDate.AddDays(-1).ToString("yyyy-MM-dd");
        string url = string.Format(apiUrl, formattedDate);

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response;

            // Retry mechanism to fetch the exchange rate from earlier dates if needed
            while (true)
            {
                response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    break; // Exit loop if the request is successful
                }
                else
                {
                    // If not successful, try the previous day
                    requestedDate = requestedDate.AddDays(-1);
                    formattedDate = requestedDate.ToString("yyyy-MM-dd");
                    url = string.Format(apiUrl, formattedDate);
                }
            }

            // Parse the XML response from NBP
            string xmlContent = await response.Content.ReadAsStringAsync();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            // Extract necessary fields from the XML
            XmlNode midNode = xmlDoc.SelectSingleNode("/ExchangeRatesSeries/Rates/Rate/Mid");
            XmlNode tableNode = xmlDoc.SelectSingleNode("/ExchangeRatesSeries/No");
            XmlNode effectiveDateNode = xmlDoc.SelectSingleNode("/ExchangeRatesSeries/EffectiveDate");

            if (midNode == null || tableNode == null || effectiveDateNode == null)
            {
                throw new Exception("Failed to retrieve necessary data from NBP API.");
            }

            // Convert mid value to decimal
            string midValueStr = midNode.InnerText.Replace(".", ",");
            if (decimal.TryParse(midValueStr, NumberStyles.Any, CultureInfo.GetCultureInfo("pl-PL"), out decimal midValue))
            {
                midValue = Math.Round(midValue, 4);

                // Create the information message
                string message = $"Kwota VAT została przeliczona na złote polskie po kursie średnim NBP dla EUR, " +
                                 $"Tabela nr. {tableNode.InnerText} z {effectiveDateNode.InnerText}.";

                // Return the result as an object containing both the rate and the message
                return new EuroRateResult
                {
                    Rate = midValue,
                    Message = message
                };
            }
            else
            {
                throw new Exception("Failed to convert the Euro exchange rate to a decimal value.");
            }
        }
    }
}
public class EuroRateResult
{
public decimal Rate { get; set; }
public string Message { get; set; }
}

