// filepath: Models/AppConfig.cs
namespace TwitchNotifier.Models;

/// <summary>
/// 應用程式設定資訊
/// </summary>
public record AppConfig(
    string ClientId,
    string ClientSecret,
    List<string> Channels,
    int InitialPollInterval = 1)
{
    /// <summary>
    /// 目前的輪詢間隔 (分鐘)
    /// </summary>
    public int PollIntervalInMinutes { get; set; } = InitialPollInterval;
}
