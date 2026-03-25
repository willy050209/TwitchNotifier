# TwitchNotifier 🎬

一個簡單、高效且安全的 C# 控制台程式，可以同時監控多個 Twitch 頻道的開台狀態。一旦偵測到開台，會自動開啟瀏覽器跳轉至該實況。

## ✨ 特色
- **多頻道監控**：支援同時監控多個頻道（最多 100 個）。
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
   ```

### 3. 編譯與執行
確保你已安裝 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。

```bash
# 還原套件
dotnet restore

# 編譯並執行
dotnet run --project TwitchNotifier/TwitchNotifier.csproj
```

## 🛠️ 技術細節
- **架構**：使用相依性注入 (DI) 與服務層架構。
- **通訊**：使用 `System.Net.Http.Json` 進行強型別 API 互動。
- **非同步**：全非同步 (Async/Await) 設計，不阻塞執行緒。

## 📄 授權 (License)
本專案採用 [MIT License](LICENSE)。
