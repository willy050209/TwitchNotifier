// filepath: Models/ChannelConfig.cs
using Avalonia.Media.Imaging;

namespace TwitchNotifier.UI.Models;

/// <summary>
/// 頻道設定資訊
/// </summary>
public partial class ChannelConfig : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _profileImageUrl = string.Empty;

    [ObservableProperty]
    [JsonIgnore]
    private Bitmap? _profileImage;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    [JsonIgnore]
    private bool _isLive;

    public ChannelConfig() { }

    public ChannelConfig(string name, bool isEnabled = true)
    {
        Name = name;
        DisplayName = name;
        IsEnabled = isEnabled;
    }
}
