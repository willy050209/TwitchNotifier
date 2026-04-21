// filepath: Converters/LiveTextConverter.cs
using Avalonia.Data.Converters;
using System.Globalization;

namespace TwitchNotifier.UI.Converters;

public class LiveTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isLive)
        {
            return isLive ? "直播中" : "離線";
        }
        return "未知";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
