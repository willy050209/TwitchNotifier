// filepath: Models/TwitchAuthResponse.cs
namespace TwitchNotifier.Models;

/// <summary>
/// Twitch OAuth2 Token 回應
/// </summary>
public record TwitchAuthResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType);
