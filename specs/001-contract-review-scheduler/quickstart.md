# 契約審查預約系統 - 快速開始指南

**日期**: 2025-11-18  
**目標讀者**: .NET 開發人員
**技術棧**: ASP.NET Core 8.0 + SQL Server

## 概述

本指南將帶您快速設置並運行契約審查預約系統的開發環境。此專案是純後端 REST API，使用 ASP.NET Core 8.0 和 SQL Server。

## 必要條件

確保您已安裝以下軟體：

- **.NET SDK**: 8.0 或更高版本 ([下載](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Visual Studio**: 2022 或更高版本（建議 Enterprise），或 VS Code + C# 擴充
- **SQL Server**: 2019 或更高版本（開發環境可使用 SQL Server Express 或 LocalDB）
- **Git**: 2.x 或更高版本
- **Postman 或 Thunder Client**: API 測試工具

### 選擇性工具
- **SQL Server Management Studio (SSMS)**: 資料庫管理工具
- **Azure Data Studio**: 輕量級 SQL Server 管理工具

## 開發環境設置

### 方式 1：使用 Visual Studio 2022 (推薦)

#### 1. 開啟專案

```bash
git clone <repository-url>
cd ContractReviewScheduler
```

在 Visual Studio 中開啟 `ContractReviewScheduler.sln`。

#### 2. 復原 NuGet 套件

Visual Studio 會自動復原套件，或手動執行：

```bash
dotnet restore
```

#### 3. 配置資料庫連接

編輯 `appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContractReviewDb;Trusted_Connection=true;Encrypt=false;"
  },
  "Ldap": {
    "Path": "LDAP://your-ad-server:389",
    "BaseDn": "dc=company,dc=com"
  },
  "Smtp": {
    "Host": "mail.company.com",
    "Port": 587,
    "EnableSSL": true,
    "Username": "noreply@isn.co.jp",
    "Password": "your-smtp-password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-chars-long-!!!",
    "ExpirationMinutes": 60
  }
}
```

#### 4. 建立資料庫與遷移

在 Package Manager Console 執行：

```powershell
Add-Migration InitialCreate
Update-Database
```

或使用 .NET CLI：

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 5. 執行應用

按 F5 或使用 Visual Studio 的「開始偵錯」按鈕。

API 將在 `https://localhost:7001` (HTTPS) 或 `http://localhost:5001` (HTTP) 運行。

### 方式 2：使用命令列 (VS Code)

#### 1. 準備開發環境

```bash
git clone <repository-url>
cd ContractReviewScheduler
```

#### 2. 復原依賴

```bash
dotnet restore
```

#### 3. 配置連接字串

建立或編輯 `appsettings.Development.json` (如上所示)

#### 4. 執行資料庫遷移

```bash
dotnet ef database update
```

#### 5. 執行應用

```bash
dotnet run --configuration Development
```

API 將在 `https://localhost:7001` 運行。

### 方式 3：使用 Docker (可選)

#### 1. 建立 Dockerfile

在專案根目錄建立 `Dockerfile`：

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "ContractReviewScheduler.dll"]
```

#### 2. 建立 docker-compose.yml

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: YourPassword123!
      ACCEPT_EULA: Y
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  api:
    build: .
    ports:
      - "5001:80"
      - "7001:443"
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=ContractReviewDb;User Id=sa;Password=YourPassword123!;"
    depends_on:
      - sqlserver
    volumes:
      - .:/src

volumes:
  sqlserver_data:
```

#### 3. 執行服務

```bash
docker-compose up -d
```

## 專案結構

```
ContractReviewScheduler/
├── ContractReviewScheduler.csproj
├── Program.cs                        # 應用入點
├── appsettings.json
├── appsettings.Development.json
│
├── Controllers/
│   ├── AuthController.cs             # 認證端點
│   ├── AppointmentsController.cs      # 預約端點
│   ├── LeaveSchedulesController.cs    # 休假端點
│   └── CalendarController.cs          # 月曆端點
│
├── Models/
│   ├── Domain/                       # 領域模型
│   │   ├── User.cs
│   │   ├── Appointment.cs
│   │   ├── LeaveSchedule.cs
│   │   └── AppointmentHistory.cs
│   │
│   └── Dto/                          # 資料傳輸物件 (POCO)
│       ├── LoginRequest.cs
│       ├── AppointmentResponse.cs
│       └── ...
│
├── Data/
│   ├── ApplicationDbContext.cs        # DbContext 定義
│   └── Migrations/                   # EF Core 遷移檔案
│
├── Services/
│   ├── IAuthService.cs / AuthService.cs
│   ├── IAppointmentService.cs / AppointmentService.cs
│   ├── ILdapService.cs / LdapService.cs
│   ├── IEmailService.cs / EmailService.cs
│   └── ILeaveScheduleService.cs / LeaveScheduleService.cs
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── AuthenticationMiddleware.cs
│
├── HostedServices/
│   └── EmailQueueService.cs          # 後台郵件處理
│
├── Validators/
│   └── AppointmentValidator.cs       # 商業邏輯驗證
│
├── Repositories/ (可選)
│   ├── IAppointmentRepository.cs
│   └── AppointmentRepository.cs
│
└── Tests/
    ├── UnitTests/
    │   ├── AuthServiceTests.cs
    │   └── AppointmentServiceTests.cs
    └── IntegrationTests/
        └── AppointmentControllerTests.cs
```

## 常用命令

### Entity Framework Core 命令

```bash
# 建立新遷移
dotnet ef migrations add <MigrationName>

# 套用最新遷移
dotnet ef database update

# 移除最後一個遷移
dotnet ef migrations remove

# 生成資料庫腳本
dotnet ef migrations script

# 查看現有遷移
dotnet ef migrations list
```

### 編譯和執行

```bash
# 編譯
dotnet build

# 編譯 Release 版本
dotnet build -c Release

# 執行
dotnet run

# 執行特定設定
dotnet run --configuration Development

# 執行特定啟動設定檔
dotnet run --launch-profile "https"
```

### 測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試類別
dotnet test --filter "ClassName=AuthServiceTests"

# 產生程式碼涵蓋率報告
dotnet test /p:CollectCoverage=true
```

### 打包和發佈

```bash
# 建立 Release 組建
dotnet publish -c Release -o ./publish

# 建立自含應用
dotnet publish -c Release -r win-x64 --self-contained
```

## 資料庫連接

### 使用 SQL Server Management Studio (SSMS)

1. 開啟 SSMS
2. 伺服器名稱: `localhost` 或 `(localdb)\mssqllocaldb` (使用 LocalDB 時)
3. 認證: Windows 認證
4. 連接

### 使用 Azure Data Studio

```bash
# 安裝 Azure Data Studio
# 連接資訊與 SSMS 相同
```

## API 測試

### 使用 Postman

1. 下載 [Postman](https://www.postman.com/downloads/)
2. 匯入 API 集合 (位於 `postman/ContractReviewScheduler.postman_collection.json`)
3. 開始測試 API

### 使用 Thunder Client (VS Code)

1. 安裝 Thunder Client 擴充
2. 在 VS Code 側邊欄開啟 Thunder Client
3. 建立新的 HTTP 要求
4. 測試範例:

```bash
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "john.doe",
  "password": "password123"
}
```

### 使用 curl

```bash
# 登入
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john.doe","password":"password123"}'

# 建立預約
curl -X POST http://localhost:5001/api/appointments \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{...}'
```

## 常見問題

### Q: 如何重置資料庫?

```bash
# 刪除所有遷移並重新建立
dotnet ef database drop
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Q: 如何連接到本機 SQL Server?

連接字串範例:

```
Server=localhost;Database=ContractReviewDb;Integrated Security=true;Encrypt=false;
```

### Q: 如何啟用 SQL 查詢日誌?

編輯 `Program.cs`：

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging();
});
```

### Q: 如何除錯 LDAP 認證?

在 `LdapService` 新增詳細日誌：

```csharp
_logger.LogInformation($"嘗試連接 LDAP: {_ldapPath}");
_logger.LogInformation($"搜尋使用者: {username}");
```

### Q: API 無法啟動怎麼辦?

1. 檢查 `appsettings.Development.json` 設定
2. 確認 SQL Server 運行中
3. 檢查連接字串是否正確
4. 查看詳細錯誤訊息

### Q: 如何查看 Serilog 日誌?

日誌會輸出到:
- 控制台 (開發模式)
- 檔案 (配置中指定)
- 事件檢視器 (配置中指定)

### Q: 如何管理用戶端密鑰?

使用 `.NET User Secrets`:

```bash
# 初始化
dotnet user-secrets init

# 設定密鑰
dotnet user-secrets set "Ldap:Password" "your-password"

# 列出所有密鑰
dotnet user-secrets list
```

## 憲章合規性檢查清單

開發時請確保符合以下要求：

- ✅ 所有 API 錯誤訊息使用繁體中文
- ✅ 所有 API 文件使用繁體中文
- ✅ 程式碼註釋使用繁體中文
- ✅ 單元測試涵蓋率 >= 80%
- ✅ 函式循環複雜度 <= 10
- ✅ API 回應時間 < 200ms
- ✅ 實施安全認證和授權
- ✅ 實施 OWASP 安全最佳實踐

## 下一步

1. 閱讀 [API 文件](contracts/openapi.yaml)
2. 檢視 [資料模型](data-model.md)
3. 查看 [研究文件](research.md)
4. 開始實作任務 (見 `tasks.md`)

## 支援

如有問題或需要幫助，請：

1. 查看相關文件
2. 檢查 GitHub Issues
3. 聯絡開發團隊

---

**最後更新**: 2025-11-18