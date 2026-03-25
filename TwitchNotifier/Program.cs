// filepath: Program.cs
using System.Diagnostics;
using TwitchNotifier.Models;
using TwitchNotifier.Services;
using dotenv.net;

// 載入 .env 檔案
DotEnv.Load();

// 設定環境變數讀取
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

// 驗證必要的設定
var clientId = configuration["TWITCH_CLIENT_ID"];
var clientSecret = configuration["TWITCH_CLIENT_SECRET"];
var targetChannelsRaw = configuration["TWITCH_TARGET_CHANNEL"];
var pollIntervalStr = configuration["TWITCH_POLL_INTERVAL"];

if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(targetChannelsRaw))
{
    Console.WriteLine("錯誤: 請設定環境變數 TWITCH_CLIENT_ID, TWITCH_CLIENT_SECRET 與 TWITCH_TARGET_CHANNEL (以逗號分隔)。");
    return;
}

int pollInterval = 1; // 預設 1 分鐘
if (int.TryParse(pollIntervalStr, out int parsedInterval) && parsedInterval > 0)
{
    pollInterval = parsedInterval;
}

var channels = targetChannelsRaw
    .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToList();

var config = new AppConfig(clientId, clientSecret, channels, pollInterval);

// 設定依賴注入
var services = new ServiceCollection();
services.AddSingleton(config);
services.AddHttpClient("TwitchApi", c => c.BaseAddress = new Uri("https://api.twitch.tv/helix/"));
services.AddHttpClient("TwitchAuth", c => c.BaseAddress = new Uri("https://id.twitch.tv/oauth2/"));
services.AddSingleton<TwitchService>();

var serviceProvider = services.BuildServiceProvider();
var twitchService = serviceProvider.GetRequiredService<TwitchService>();

Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 開始監控頻道: {string.Join(", ", config.Channels)}...");
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 目前輪詢間隔: {config.PollIntervalInMinutes} 分鐘");

// 使用字典追蹤每個頻道的狀態
var channelStates = config.Channels.ToDictionary(c => c.ToLowerInvariant(), _ => false);

async Task PerformCheck()
{
    try
    {
        var liveChannels = await twitchService.GetLiveChannelsAsync(channelStates.Keys);

        foreach (var channelName in channelStates.Keys.ToList())
        {
            var isCurrentlyLive = liveChannels.Contains(channelName);
            var wasAlreadyLive = channelStates[channelName];

            if (isCurrentlyLive && !wasAlreadyLive)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 發現 {channelName} 開台了！正在開啟瀏覽器...");
                OpenBrowser($"https://www.twitch.tv/{channelName}");
                channelStates[channelName] = true;
            }
            else if (!isCurrentlyLive && wasAlreadyLive)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {channelName} 已關台。");
                channelStates[channelName] = false;
            }
        }

        var liveCount = channelStates.Values.Count(isLive => isLive);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 狀態檢查完成。目前監控中: {channelStates.Count} 個頻道 (正在直播: {liveCount})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 檢查狀態時發生錯誤: {ex.Message}");
    }
}

// 1. 啟動時先立即檢查一次
await PerformCheck();

// 2. 進入計時迴圈
var currentTimerInterval = config.PollIntervalInMinutes;
var timer = new PeriodicTimer(TimeSpan.FromMinutes(currentTimerInterval));

try
{
    while (await timer.WaitForNextTickAsync())
    {
        // 動態更新設定：重新讀取 .env
        DotEnv.Load();
        var refreshConfig = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        
        // 更新頻道清單
        var updatedChannelsRaw = refreshConfig["TWITCH_TARGET_CHANNEL"];
        if (!string.IsNullOrEmpty(updatedChannelsRaw))
        {
            var updatedChannels = updatedChannelsRaw
                .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(c => c.ToLowerInvariant())
                .ToList();

            config.Channels.Clear();
            config.Channels.AddRange(updatedChannels);

            foreach (var ch in updatedChannels.Where(ch => !channelStates.ContainsKey(ch)))
            {
                channelStates[ch] = false;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 動態新增監控清單: {ch}");
            }
            var removed = channelStates.Keys.Where(ch => !updatedChannels.Contains(ch)).ToList();
            foreach (var ch in removed)
            {
                channelStates.Remove(ch);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 動態移除監控清單: {ch}");
            }
        }

        // 動態更新輪詢間隔
        var updatedPollIntervalStr = refreshConfig["TWITCH_POLL_INTERVAL"];
        if (int.TryParse(updatedPollIntervalStr, out int updatedInterval) && updatedInterval > 0)
        {
            if (updatedInterval != currentTimerInterval)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 偵測到輪詢間隔變更: {currentTimerInterval} -> {updatedInterval} 分鐘");
                currentTimerInterval = updatedInterval;
                config.PollIntervalInMinutes = updatedInterval;
                
                // 重置計時器
                timer.Dispose();
                timer = new PeriodicTimer(TimeSpan.FromMinutes(currentTimerInterval));
            }
        }

        await PerformCheck();
    }
}
finally
{
    timer.Dispose();
}

static void OpenBrowser(string url)
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"無法開啟瀏覽器: {ex.Message}");
    }
}
