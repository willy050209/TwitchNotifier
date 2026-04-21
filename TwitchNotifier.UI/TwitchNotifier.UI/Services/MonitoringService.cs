// filepath: Services/MonitoringService.cs
using System.Diagnostics;

namespace TwitchNotifier.UI.Services;

/// <summary>
/// 背景監控服務，負責輪詢 Twitch API 並觸發通知
/// </summary>
public class MonitoringService(TwitchService twitchService, AppConfig config)
{
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    public event Action<string>? OnChannelLive;
    public event Action? OnStatusUpdated;

    /// <summary>
    /// 啟動監控
    /// </summary>
    public void Start(IEnumerable<ChannelConfig> channels)
    {
        Stop();
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(config.PollIntervalInMinutes));
        
        _ = RunLoop(channels, _cts.Token);
    }

    /// <summary>
    /// 停止監控
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _timer?.Dispose();
    }

    private async Task RunLoop(IEnumerable<ChannelConfig> channels, CancellationToken ct)
    {
        try
        {
            // 首次檢查
            await CheckChannels(channels, ct);

            while (await _timer!.WaitForNextTickAsync(ct))
            {
                await CheckChannels(channels, ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task CheckChannels(IEnumerable<ChannelConfig> channels, CancellationToken ct)
    {
        var channelList = channels.ToList();
        var targetNames = channelList.Where(c => c.IsEnabled).Select(c => c.Name).ToList();
        
        if (targetNames.Count == 0) return;

        try
        {
            var liveNames = await twitchService.GetLiveChannelsAsync(targetNames, ct);

            foreach (var channel in channelList)
            {
                var isCurrentlyLive = liveNames.Contains(channel.Name.ToLowerInvariant());
                
                if (isCurrentlyLive && !channel.IsLive)
                {
                    // 剛開台
                    OnChannelLive?.Invoke(channel.Name);
                    OpenBrowser($"https://www.twitch.tv/{channel.Name}");
                }
                
                channel.IsLive = isCurrentlyLive;
            }

            OnStatusUpdated?.Invoke();
        }
        catch (Exception)
        {
            // 忽略錯誤，等待下次輪詢
        }
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch { }
    }
}
