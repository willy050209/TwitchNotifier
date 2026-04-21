// filepath: Converters/LiveColorConverter.cs
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace TwitchNotifier.UI.Converters;

public class LiveColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isLive)
        {
            return isLive ? Brushes.Red : Brushes.Gray;
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
