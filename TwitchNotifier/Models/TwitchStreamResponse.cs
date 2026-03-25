// filepath: Models/TwitchStreamResponse.cs
namespace TwitchNotifier.Models;

/// <summary>
/// Twitch 實況狀態回應
/// </summary>
public record TwitchStreamResponse(
    [property: JsonPropertyName("data")] List<TwitchStreamData> Data);

/// <summary>
/// Twitch 實況資料
/// </summary>
public record TwitchStreamData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("user_login")] string UserLogin,
    [property: JsonPropertyName("user_name")] string UserName,
    [property: JsonPropertyName("game_name")] string GameName,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("viewer_count")] int ViewerCount,
    [property: JsonPropertyName("started_at")] DateTime StartedAt);
