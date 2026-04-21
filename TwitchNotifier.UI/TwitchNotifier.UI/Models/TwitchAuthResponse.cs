// filepath: Models/TwitchAuthResponse.cs
namespace TwitchNotifier.UI.Models;

public record TwitchAuthResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType);
