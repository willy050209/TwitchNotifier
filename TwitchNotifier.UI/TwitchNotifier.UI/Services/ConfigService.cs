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
    private readonly string _basePath;
    private readonly string _channelsPath;
    private readonly string _envPath;

    public ConfigService()
    {
        // 尋找專案根目錄 (有 .env 的地方)
        _basePath = FindProjectRoot(AppDomain.CurrentDomain.BaseDirectory);
        _channelsPath = Path.Combine(_basePath, ChannelsFileName);
        _envPath = Path.Combine(_basePath, ".env");
        
        Console.WriteLine($"[Config] 使用工作目錄: {_basePath}");
    }

    private static string FindProjectRoot(string startDir)
    {
        var current = new DirectoryInfo(startDir);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, ".env")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        // 若都沒找到，就用執行目錄
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// 載入 API 設定
    /// </summary>
    public AppConfig LoadAppConfig()
    {
        if (File.Exists(_envPath))
        {
            DotEnv.Load(new DotEnvOptions(envFilePaths: [_envPath]));
        }
        else
        {
            DotEnv.Load(); // 嘗試預設搜尋
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
    /// 載入頻道清單
    /// </summary>
    public List<ChannelConfig> LoadChannels()
    {
        if (File.Exists(_channelsPath))
        {
            try
            {
                var json = File.ReadAllText(_channelsPath);
                var list = JsonSerializer.Deserialize<List<ChannelConfig>>(json);
                if (list != null && list.Count > 0)
                {
                    Console.WriteLine($"[Config] 已從 {_channelsPath} 載入 {list.Count} 個頻道");
                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 讀取 JSON 失敗: {ex.Message}");
            }
        }

        // 嘗試從環境變數遷移 (僅在 JSON 不存在時)
        var envChannelsRaw = Environment.GetEnvironmentVariable("TWITCH_TARGET_CHANNEL");
        if (!string.IsNullOrEmpty(envChannelsRaw))
        {
            Console.WriteLine("[Config] 偵測到舊版 .env 設定，執行自動遷移...");
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
        try
        {
            var json = JsonSerializer.Serialize(channels, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_channelsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] 儲存失敗: {ex.Message}");
        }
    }
}
