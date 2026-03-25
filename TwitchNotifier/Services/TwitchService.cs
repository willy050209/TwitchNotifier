// filepath: Services/TwitchService.cs
using TwitchNotifier.Models;

namespace TwitchNotifier.Services;

/// <summary>
/// 處理 Twitch API 互動的服務
/// </summary>
public class TwitchService(IHttpClientFactory httpClientFactory, AppConfig config)
{
    private string? _accessToken;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 取得目前正在直播的頻道清單
    /// </summary>
    /// <returns>回傳包含正在直播的頻道名稱 (小寫) 的 HashSet</returns>
    public async Task<HashSet<string>> GetLiveChannelsAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            await RefreshTokenAsync(ct);
        }

        var client = httpClientFactory.CreateClient("TwitchApi");
        
        // 同時查詢多個頻道，並將上限設為 100
        var queryParams = string.Join("&", config.Channels.Select(c => $"user_login={Uri.EscapeDataString(c.ToLowerInvariant())}"));
        var request = new HttpRequestMessage(HttpMethod.Get, $"streams?{queryParams}&first=100");
        
        request.Headers.Add("Client-Id", config.ClientId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await client.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await RefreshTokenAsync(ct);
            return await GetLiveChannelsAsync(ct);
        }

        response.EnsureSuccessStatusCode();

        var streamResponse = await response.Content.ReadFromJsonAsync<TwitchStreamResponse>(JsonOptions, ct);
        
        // 修正：比對 UserLogin 欄位 (這是唯一的登入帳號名，且 API 保證為小寫)
        return streamResponse?.Data?
            .Select(d => d.UserLogin.ToLowerInvariant())
            .ToHashSet() ?? [];
    }

    private async Task RefreshTokenAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("TwitchAuth");

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", config.ClientId),
            new KeyValuePair<string, string>("client_secret", config.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        ]);

        var response = await client.PostAsync("token", content, ct);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<TwitchAuthResponse>(JsonOptions, ct);
        _accessToken = authResponse?.AccessToken ?? throw new InvalidOperationException("無法取得 Access Token");
    }
}
