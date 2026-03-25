// filepath: Models/AppConfig.cs
namespace TwitchNotifier.Models;

/// <summary>
/// 應用程式設定資訊
/// </summary>
/// <param name="ClientId">Twitch Client ID</param>
/// <param name="ClientSecret">Twitch Client Secret</param>
/// <param name="Channels">監控頻道清單</param>
/// <param name="PollIntervalInMinutes">輪詢間隔 (分鐘)</param>
public record AppConfig(
    string ClientId,
    string ClientSecret,
    IReadOnlyList<string> Channels,
    int PollIntervalInMinutes = 3);
