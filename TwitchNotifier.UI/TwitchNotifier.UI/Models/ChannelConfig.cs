// filepath: Models/ChannelConfig.cs
namespace TwitchNotifier.UI.Models;

/// <summary>
/// 頻道設定資訊 (具備屬性變更通知)
/// </summary>
public partial class ChannelConfig : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    [JsonIgnore]
    private bool _isLive;

    public ChannelConfig() { }

    public ChannelConfig(string name, bool isEnabled = true)
    {
        Name = name;
        IsEnabled = isEnabled;
    }
}
