// filepath: Models/TwitchStreamResponse.cs
namespace TwitchNotifier.UI.Models;

public record TwitchStreamResponse(
    [property: JsonPropertyName("data")] List<StreamData> Data);

public record StreamData(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("user_login")] string UserLogin,
    [property: JsonPropertyName("user_name")] string UserName,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("viewer_count")] int ViewerCount,
    [property: JsonPropertyName("started_at")] DateTime StartedAt,
    [property: JsonPropertyName("language")] string Language);
