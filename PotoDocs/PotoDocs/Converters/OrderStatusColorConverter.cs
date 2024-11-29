using System.Globalization;

namespace PotoDocs.Converters;

public class OrderStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderDto order)
        {
            if (order.CMRFiles == null || order.CMRFiles.Count < 1 || !(order.HasPaid ?? false))
            {
                return Colors.Red;
            }
            return Colors.Green;
        }

        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
