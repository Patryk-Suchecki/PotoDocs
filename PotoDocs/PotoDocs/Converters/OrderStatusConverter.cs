using System.Globalization;

namespace PotoDocs.Converters;

public class OrderStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderDto order)
        {
            if (order.CMRFiles == null || order.CMRFiles.Count < 1)
            {
                return "Brak CMR";
            }
            return order.HasPaid ? "Zapłacono" : "Nie zapłacono";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}