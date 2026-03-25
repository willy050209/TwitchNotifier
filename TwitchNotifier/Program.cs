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

if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(targetChannelsRaw))
{
    Console.WriteLine("錯誤: 請設定環境變數 TWITCH_CLIENT_ID, TWITCH_CLIENT_SECRET 與 TWITCH_TARGET_CHANNEL (以逗號分隔)。");
    return;
}

// 支援以逗號、分號或空白分隔多個頻道
var channels = targetChannelsRaw
    .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToList();

var config = new AppConfig(clientId, clientSecret, channels);

// 設定依賴注入
var services = new ServiceCollection();
services.AddSingleton(config);
services.AddHttpClient("TwitchApi", c => c.BaseAddress = new Uri("https://api.twitch.tv/helix/"));
services.AddHttpClient("TwitchAuth", c => c.BaseAddress = new Uri("https://id.twitch.tv/oauth2/"));
services.AddSingleton<TwitchService>();

var serviceProvider = services.BuildServiceProvider();
var twitchService = serviceProvider.GetRequiredService<TwitchService>();

Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 開始監控頻道: {string.Join(", ", config.Channels)}...");

// 使用字典追蹤每個頻道的狀態
var channelStates = config.Channels.ToDictionary(c => c.ToLowerInvariant(), _ => false);

async Task PerformCheck()
{
    try
    {
        var liveChannels = await twitchService.GetLiveChannelsAsync();

        foreach (var channelName in channelStates.Keys)
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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 檢查狀態時發生錯誤: {ex.Message}");
    }
}

// 1. 啟動時先立即檢查一次
await PerformCheck();

// 2. 進入計時迴圈
using var timer = new PeriodicTimer(TimeSpan.FromMinutes(config.PollIntervalInMinutes));
while (await timer.WaitForNextTickAsync())
{
    await PerformCheck();
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
