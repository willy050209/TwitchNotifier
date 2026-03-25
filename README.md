# TwitchNotifier 🎬

一個簡單、高效且安全的 C# 控制台程式，可以同時監控多個 Twitch 頻道的開台狀態。一旦偵測到開台，會自動開啟瀏覽器跳轉至該實況。

## ✨ 特色
- **多頻道監控**：支援同時監控多個頻道（最多 100 個）。
- **動態熱更新**：支援在不停止程式的情況下，直接修改 `.env` 檔案來增減監控頻道或調整輪詢時間。
- **極速偵測**：程式啟動即檢查，隨後按設定間隔持續輪詢。
- **安全性高**：敏感資訊（Client ID / Secret）透過環境變數管理，不留存在程式碼中。
- **低資源消耗**：使用 .NET 10 的 `PeriodicTimer` 與 `IHttpClientFactory` 優化效能。
- **現代 C# 語法**：採用 C# 14 與 .NET 10 標準編寫。

## 🚀 快速開始

### 1. 取得 Twitch API 憑證
1. 前往 [Twitch Developers Console](https://dev.twitch.tv/console)。
2. 建立一個新的應用程式 (Application)。
3. 取得你的 **Client ID** 與 **Client Secret**。

### 2. 環境設定
1. 在專案目錄下建立一個名為 `.env` 的檔案。
2. 參考 `.env.example` 填入你的憑證：
   ```env
   TWITCH_CLIENT_ID=你的_CLIENT_ID
   TWITCH_CLIENT_SECRET=你的_CLIENT_SECRET
   TWITCH_TARGET_CHANNEL=頻道A, 頻道B, 頻道C
   TWITCH_POLL_INTERVAL=3
   ```

### 3. 編譯與執行
確保你已安裝 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。

```bash
# 編譯並執行
dotnet run --project TwitchNotifier/TwitchNotifier.csproj
```

## 動態熱更新
本程式支援 **熱更新 (Hot Reload)**。當程式正在執行時，你可以隨時打開 `.env` 檔案並修改以下內容：

1. **增減頻道**：修改 `TWITCH_TARGET_CHANNEL`，例如增加 `margaretnorth`。
2. **調整頻率**：修改 `TWITCH_POLL_INTERVAL`（單位：分鐘），例如從 `3` 改成 `1` 讓檢查更頻繁。
3. **儲存檔案**：儲存後，程式會在下一次輪詢時自動套用新設定，**無需重新啟動程式**。

## 📄 授權 (License)
本專案採用 [MIT License](LICENSE)。
