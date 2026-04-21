# TwitchNotifier 🎬

一個簡單、高效且安全的 C# 應用程式，可以同時監控多個 Twitch 頻道的開台狀態。一旦偵測到開台，會自動開啟瀏覽器跳轉至該實況。

## ✨ 特色
- **📺 現代化 UI 管理**：全新 Avalonia UI 介面，視覺化管理頻道清單。
- **⚙️ 靈活管理**：支援在介面上直接新增、刪除頻道，並具備「監控中」開關，隨時啟用或停用個別頻道。
- **多頻道監控**：支援同時監控多個頻道（最多 100 個）。
- **極速偵測**：程式啟動即檢查，隨後按設定間隔持續輪詢。
- **安全性高**：敏感資訊 (Client ID / Secret) 透過 `.env` 環境變數管理。
- **低資源消耗**：使用 .NET 10 的 `PeriodicTimer` 與 `IHttpClientFactory` 優化效能。
- **現代 C# 語法**：採用 C# 14 與 .NET 10 標準編寫。

## 🚀 快速開始

### 1. 取得 Twitch API 憑證
1. 前往 [Twitch Developers Console](https://dev.twitch.tv/console)。
2. 建立一個新的應用程式 (Application)。
3. 取得你的 **Client ID** 與 **Client Secret**。

### 2. 環境設定
1. 在 `TwitchNotifier.UI/TwitchNotifier.UI.Desktop` 目錄（或專案根目錄）下建立一個名為 `.env` 的檔案。
2. 參考 `.env.example` 填入你的憑證：
   ```env
   TWITCH_CLIENT_ID=你的_CLIENT_ID
   TWITCH_CLIENT_SECRET=你的_CLIENT_SECRET
   TWITCH_POLL_INTERVAL=3
   ```
   *(註：原本的 `TWITCH_TARGET_CHANNEL` 頻道清單現在改由 UI 中的 `channels.json` 統一管理。)*

### 3. 編譯與執行 (UI 模式 - 推薦)
確保你已安裝 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。

```bash
# 執行 UI 管理員
dotnet run --project TwitchNotifier.UI/TwitchNotifier.UI.Desktop/TwitchNotifier.UI.Desktop.csproj
```

### 4. 編譯與執行 (Console 模式)
如果你偏好純文字模式，仍可執行原本的控制台程式：

```bash
dotnet run --project TwitchNotifier/TwitchNotifier.csproj
```

## 🛠️ 功能說明
- **新增/刪除頻道**：在 UI 頂部輸入頻道名稱後點擊「新增」，即可將其加入監控名單。
- **監控開關**：勾選「監控中」核取方塊，可決定該頻道是否參與目前的輪詢檢查。
- **狀態顯示**：紅色圓點表示該頻道「直播中」，灰色則為「離線」。
- **自動開啟**：一旦偵測到已啟用的頻道開台，系統會自動呼叫預設瀏覽器跳轉至該 Twitch 實況頁面。

## 📂 設定檔說明
- **`.env`**：儲存敏感的金鑰資訊。
- **`channels.json`**：(自動產生) 儲存您的頻道清單及其啟用狀態。

## 📄 授權 (License)
本專案採用 [MIT License](LICENSE)。
