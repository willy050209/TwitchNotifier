// filepath: ViewModels/MainViewModel.cs
namespace TwitchNotifier.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ConfigService _configService;
    private readonly MonitoringService _monitoringService;
    private readonly AppConfig _appConfig;

    [ObservableProperty]
    private string _newChannelName = string.Empty;

    [ObservableProperty]
    private string _statusText = "準備就緒";

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
        
        var twitchService = new TwitchService(httpClientFactory, _appConfig);
        _monitoringService = new MonitoringService(twitchService, _appConfig);

        var loadedChannels = _configService.LoadChannels();
        Channels = new ObservableCollection<ChannelConfig>(loadedChannels);
        
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

        StartMonitoring();
    }

    [RelayCommand]
    private void AddChannel()
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
