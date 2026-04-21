// filepath: Services/TwitchService.cs
namespace TwitchNotifier.UI.Services;

/// <summary>
/// 處理 Twitch API 互動的服務
/// </summary>
/// <param name="httpClientFactory">HTTP 客戶端處理工廠</param>
/// <param name="config">API 設定</param>
public class TwitchService(IHttpClientFactory httpClientFactory, AppConfig config)
{
    private string? _accessToken;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 取得目前正在直播的頻道清單
    /// </summary>
    /// <param name="channels">要查詢的頻道名稱清單</param>
    /// <param name="ct">取消權杖</param>
    /// <returns>回傳包含正在直播的頻道名稱 (小寫) 的 HashSet</returns>
    public async Task<HashSet<string>> GetLiveChannelsAsync(IEnumerable<string> channels, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(channels);
        
        var channelList = channels.ToList();
        if (channelList.Count == 0) return [];

        if (string.IsNullOrEmpty(_accessToken))
        {
            await RefreshTokenAsync(ct);
        }

        using var client = httpClientFactory.CreateClient("TwitchApi");
        
        // 分批查詢，每批上限 100 個 (Twitch API 限制)
        var liveChannels = new HashSet<string>();
        foreach (var chunk in channelList.Chunk(100))
        {
            var queryParams = string.Join("&", chunk.Select(c => $"user_login={Uri.EscapeDataString(c.ToLowerInvariant())}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"streams?{queryParams}&first=100");
            
            request.Headers.Add("Client-Id", config.ClientId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await client.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenAsync(ct);
                return await GetLiveChannelsAsync(channelList, ct);
            }

            response.EnsureSuccessStatusCode();

            var streamResponse = await response.Content.ReadFromJsonAsync<TwitchStreamResponse>(JsonOptions, ct);
            if (streamResponse?.Data != null)
            {
                foreach (var d in streamResponse.Data)
                {
                    liveChannels.Add(d.UserLogin.ToLowerInvariant());
                }
            }
        }

        return liveChannels;
    }

    private async Task RefreshTokenAsync(CancellationToken ct)
    {
        using var client = httpClientFactory.CreateClient("TwitchAuth");

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
