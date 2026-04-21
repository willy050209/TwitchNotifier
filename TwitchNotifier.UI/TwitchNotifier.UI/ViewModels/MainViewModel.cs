// filepath: ViewModels/MainViewModel.cs
using System.Collections.Concurrent;
using Avalonia.Media.Imaging;

namespace TwitchNotifier.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ConfigService _configService;
    private readonly MonitoringService _monitoringService;
    private readonly TwitchService _twitchService;
    private readonly AppConfig _appConfig;

    [ObservableProperty]
    private string _newChannelName = string.Empty;

    [ObservableProperty]
    private string _statusText = "準備就緒";

    [ObservableProperty]
    private int _pollInterval;

    public ObservableCollection<ChannelConfig> Channels { get; }

    public MainViewModel()
    {
        // 這裡暫時手動初始化，後續可以改為 DI 注入
        _configService = new ConfigService();
        _appConfig = _configService.LoadAppConfig();
        
        var services = new ServiceCollection();
        services.AddHttpClient("TwitchApi", c => c.BaseAddress = new Uri("https://api.twitch.tv/helix/"));
        services.AddHttpClient("TwitchAuth", c => c.BaseAddress = new Uri("https://id.twitch.tv/oauth2/"));
        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        
        _twitchService = new TwitchService(httpClientFactory, _appConfig);
        _monitoringService = new MonitoringService(_twitchService, _appConfig);

        var loadedChannels = _configService.LoadChannels();
        Channels = new ObservableCollection<ChannelConfig>(loadedChannels);
        
        _pollInterval = _appConfig.PollIntervalInMinutes;

        foreach (var channel in Channels)
        {
            channel.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(ChannelConfig.IsEnabled))
                {
                    _configService.SaveChannels(Channels);
                    StartMonitoring();
                }
            };
        }

        _monitoringService.OnChannelLive += name => 
            Dispatcher.UIThread.InvokeAsync(() => StatusText = $"[{DateTime.Now:HH:mm:ss}] {name} 開台了！");
        
        _monitoringService.OnStatusUpdated += () => 
            Dispatcher.UIThread.InvokeAsync(() => StatusText = $"上次檢查時間: {DateTime.Now:HH:mm:ss}");

        _ = RefreshChannelInfoAsync();
        StartMonitoring();
    }

    partial void OnPollIntervalChanged(int value)
    {
        if (value <= 0) return;
        _monitoringService.UpdateInterval(value, Channels);
        StatusText = $"輪詢間隔已更新為 {value} 分鐘";
    }

    private static readonly ConcurrentDictionary<string, Bitmap> ImageCache = new();
    private static readonly HttpClient ImageHttpClient = new();

    private async Task RefreshChannelInfoAsync(IEnumerable<ChannelConfig>? targetChannels = null)
    {
        try
        {
            var listToRefresh = (targetChannels ?? Channels).ToList();
            var logins = listToRefresh.Select(c => c.Name).ToList();
            if (logins.Count == 0) return;

            var users = await _twitchService.GetUsersInfoAsync(logins);
            foreach (var user in users)
            {
                var channel = listToRefresh.FirstOrDefault(c => c.Name.Equals(user.Login, StringComparison.OrdinalIgnoreCase));
                if (channel != null)
                {
                    channel.DisplayName = user.DisplayName;
                    channel.ProfileImageUrl = user.ProfileImageUrl;
                    
                    // 背景下載圖片
                    _ = Task.Run(async () =>
                    {
                        if (string.IsNullOrEmpty(user.ProfileImageUrl)) return;
                        
                        if (ImageCache.TryGetValue(user.ProfileImageUrl, out var cached))
                        {
                            await Dispatcher.UIThread.InvokeAsync(() => channel.ProfileImage = cached);
                            return;
                        }

                        try
                        {
                            var bytes = await ImageHttpClient.GetByteArrayAsync(user.ProfileImageUrl);
                            using var ms = new System.IO.MemoryStream(bytes);
                            var bitmap = new Bitmap(ms);
                            ImageCache[user.ProfileImageUrl] = bitmap;
                            await Dispatcher.UIThread.InvokeAsync(() => channel.ProfileImage = bitmap);
                        }
                        catch { }
                    });
                }
            }
            _configService.SaveChannels(Channels);
        }
        catch (Exception ex)
        {
            StatusText = $"取得頻道資訊失敗: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddChannel()
    {
        if (string.IsNullOrWhiteSpace(NewChannelName)) return;

        var name = NewChannelName.Trim();
        if (Channels.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            NewChannelName = string.Empty;
            return;
        }

        var newChannel = new ChannelConfig(name);
        newChannel.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(ChannelConfig.IsEnabled))
            {
                _configService.SaveChannels(Channels);
                StartMonitoring();
            }
        };
        Channels.Add(newChannel);
        
        // 即時取得新頻道的資訊
        await RefreshChannelInfoAsync([newChannel]);
        
        _configService.SaveChannels(Channels);
        NewChannelName = string.Empty;
        
        // 重新啟動監控以包含新頻道
        StartMonitoring();
    }

    [RelayCommand]
    private void DeleteChannel(ChannelConfig channel)
    {
        Channels.Remove(channel);
        _configService.SaveChannels(Channels);
        StartMonitoring();
    }

    private void StartMonitoring()
    {
        if (string.IsNullOrEmpty(_appConfig.ClientId) || string.IsNullOrEmpty(_appConfig.ClientSecret))
        {
            StatusText = "請在 .env 中設定 TWITCH_CLIENT_ID 與 TWITCH_CLIENT_SECRET";
            return;
        }
        
        _monitoringService.Start(Channels);
    }
}
