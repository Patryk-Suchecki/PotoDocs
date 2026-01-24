using System.Globalization;

namespace PotoDocs.Blazor.Extensions;

public static class MoneyExtensions
{
    public static string FormatMoney(this decimal price, CurrencyType currency)
    {
        var culture = currency == CurrencyType.PLN
            ? CultureInfo.GetCultureInfo("pl-PL")
            : CultureInfo.GetCultureInfo("de-DE");

        return price.ToString("C", culture);
    }
}