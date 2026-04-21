// filepath: Models/AppConfig.cs
namespace TwitchNotifier.UI.Models;

/// <summary>
/// 應用程式 API 核心設定資訊
/// </summary>
/// <param name="ClientId">Twitch Client ID</param>
/// <param name="ClientSecret">Twitch Client Secret</param>
/// <param name="InitialPollInterval">初始輪詢間隔 (分鐘)</param>
public record AppConfig(
    string ClientId,
    string ClientSecret,
    int InitialPollInterval = 1)
{
    /// <summary>
    /// 目前的輪詢間隔 (分鐘)
    /// </summary>
    public int PollIntervalInMinutes { get; set; } = InitialPollInterval;
}
