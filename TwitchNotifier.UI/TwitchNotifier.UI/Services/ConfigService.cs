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
    private readonly string _channelsPath;
    private readonly string _envPath;

    public ConfigService()
    {
        // 強制尋找方案根目錄 (以 .sln 所在目錄為準)
        var root = FindSolutionRoot(AppDomain.CurrentDomain.BaseDirectory);
        _channelsPath = Path.Combine(root, ChannelsFileName);
        _envPath = Path.Combine(root, ".env");
        
        // 明確輸出日誌，方便除錯
        Console.WriteLine($"[Config] 專案根目錄: {root}");
        Console.WriteLine($"[Config] 設定檔路徑: {_channelsPath}");
    }

    private static string FindSolutionRoot(string startDir)
    {
        var current = new DirectoryInfo(startDir);
        while (current != null)
        {
            // 尋找 .slnx 或 .sln 檔案
            if (current.GetFiles("*.slnx").Length > 0 || current.GetFiles("*.sln").Length > 0)
            {
                return current.FullName;
            }
            current = current.Parent;
        }
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
            DotEnv.Load(); 
        }

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        return new AppConfig(config["TWITCH_CLIENT_ID"] ?? "", 
                            config["TWITCH_CLIENT_SECRET"] ?? "", 
                            int.TryParse(config["TWITCH_POLL_INTERVAL"], out int p) ? p : 1);
    }

    /// <summary>
    /// 載入頻道清單
    /// </summary>
    public List<ChannelConfig> LoadChannels()
    {
        // 1. 優先嘗試讀取現有 JSON
        if (File.Exists(_channelsPath))
        {
            try
            {
                var json = File.ReadAllText(_channelsPath);
                var list = JsonSerializer.Deserialize<List<ChannelConfig>>(json);
                if (list != null)
                {
                    Console.WriteLine($"[Config] 成功載入 {list.Count} 個頻道");
                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 讀取失敗: {ex.Message}");
            }
        }

        // 2. 只有在完全沒有 JSON 檔案時，才嘗試從 .env 遷移
        var envChannelsRaw = Environment.GetEnvironmentVariable("TWITCH_TARGET_CHANNEL");
        if (!string.IsNullOrEmpty(envChannelsRaw))
        {
            Console.WriteLine("[Config] 執行一次性遷移...");
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
            // Console.WriteLine($"[Config] 已儲存至 {_channelsPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] 儲存失敗: {ex.Message}");
        }
    }
}
