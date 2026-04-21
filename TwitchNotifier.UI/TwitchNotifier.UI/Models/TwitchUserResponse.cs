// filepath: Models/TwitchUserResponse.cs
namespace TwitchNotifier.UI.Models;

public record TwitchUserResponse(
    [property: JsonPropertyName("data")] List<UserData> Data);

public record UserData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("profile_image_url")] string ProfileImageUrl,
    [property: JsonPropertyName("description")] string Description);
