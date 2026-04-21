// filepath: Services/ConfigService.cs
using dotenv.net;
using System.IO;

namespace TwitchNotifier.UI.Services;

/// <summary>
/// 處理應用程式設定與頻道資料持久化的服務
/// </summary>
public class ConfigService
{
    private const string ChannelsFileName = "channels.json";
    
    // 使用目前工作目錄 (通常是專案根目錄) 以確保資料持久化
    private readonly string _channelsPath = Path.Combine(Environment.CurrentDirectory, ChannelsFileName);
    private readonly string _envPath = Path.Combine(Environment.CurrentDirectory, ".env");

    /// <summary>
    /// 載入 API 設定
    /// </summary>
    public AppConfig LoadAppConfig()
    {
        // 優先載入執行目錄下的 .env，若無則嘗試上層 (開發環境)
        if (!File.Exists(_envPath))
        {
            DotEnv.Load();
        }
        else
        {
            DotEnv.Load(new DotEnvOptions(envFilePaths: [_envPath]));
        }

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var clientId = config["TWITCH_CLIENT_ID"] ?? "";
        var clientSecret = config["TWITCH_CLIENT_SECRET"] ?? "";
        var pollIntervalStr = config["TWITCH_POLL_INTERVAL"];

        int pollInterval = 1;
        if (int.TryParse(pollIntervalStr, out int parsedInterval) && parsedInterval > 0)
        {
            pollInterval = parsedInterval;
        }

        return new AppConfig(clientId, clientSecret, pollInterval);
    }

    /// <summary>
    /// 載入頻道清單，並處理舊版遷移
    /// </summary>
    public List<ChannelConfig> LoadChannels()
    {
        if (File.Exists(_channelsPath))
        {
            try
            {
                var json = File.ReadAllText(_channelsPath);
                return JsonSerializer.Deserialize<List<ChannelConfig>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }

        // 嘗試從 .env 遷移
        var envChannelsRaw = Environment.GetEnvironmentVariable("TWITCH_TARGET_CHANNEL");
        if (!string.IsNullOrEmpty(envChannelsRaw))
        {
            var migrated = envChannelsRaw
                .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(name => new ChannelConfig(name, true))
                .ToList();
            
            SaveChannels(migrated);
            return migrated;
        }

        return [];
    }

    /// <summary>
    /// 儲存頻道清單至 JSON 檔案
    /// </summary>
    public void SaveChannels(IEnumerable<ChannelConfig> channels)
    {
        var json = JsonSerializer.Serialize(channels, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_channelsPath, json);
    }
}
