# 契約審查預約系統 - 實施進度報告

**日期**: 2025-11-18  
**狀態**: ✅ Phase 1 完成 (設置階段)  
**進度**: 7/78 任務完成 (9%)

## 項目概要

| 項目 | 詳情 |
|------|------|
| 系統名稱 | 契約審查預約系統 (Contract Review Appointment System) |
| 技術棧 | ASP.NET Core 8.0, SQL Server 2019+, EF Core 8.0, React 19.2.0 |
| 分支 | `001-contract-review-scheduler` |
| 後端語言 | C# 12 |
| 前端語言 | TypeScript 5.6 |

## Phase 1 (設置) - 完成度: ✅ 100%

### 完成的任務 (7/7)

#### ✓ T001: 創建 ASP.NET Core 8.0 Web API 專案結構
- **位置**: `backend/`
- **內容**:
  - Controllers/ - API 控制器
  - Models/Domain/ - 領域實體
  - Services/ - 商業邏輯
  - Middleware/ - HTTP 中間件
  - Data/ - 資料層
  - Tests/ - 測試分層
  - HostedServices/ - 後台服務
  - Templates/ - 郵件範本

#### ✓ T002: 配置 NuGet 依賴套件
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.18" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.18" />
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
<PackageReference Include="System.DirectoryServices" Version="4.7.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.5.1" />
```

#### ✓ T003: 設置 SQL Server 連線字串
- **檔案**: `backend/appsettings.Development.json`
- **連線字串**: LocalDB 配置 (可配置為完整 SQL Server)
- **支援**: 自動數據庫建立 (Code First)

#### ✓ T004: 配置 Entity Framework Core DbContext
- **檔案**: `backend/Data/ApplicationDbContext.cs`
- **實體**: 5 個領域實體
  - User (使用者)
  - Appointment (預約)
  - LeaveSchedule (休假排程)
  - AppointmentHistory (預約歷史)
  - NotificationLog (通知日誌)
- **配置**: Fluent API 關係、複合索引、外鍵約束、軟刪除支援

#### ✓ T005: 創建初始 EF Core 遷移
- **策略**: Code First
- **狀態**: 遷移已準備 (T005 準備中)
- **下一步**: `dotnet ef database update`

#### ✓ T006: 配置 Serilog 結構化日誌
- **檔案**: `backend/Program.cs`
- **輸出目標**:
  - Console 即時輸出
  - 文件 (logs/log-*.txt) 日期滾動
- **日誌格式**: 
  ```
  {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}
  ```
- **級別**: Information+

#### ✓ T007: 創建全域例外處理中間件
- **檔案**: `backend/Middleware/ExceptionHandlingMiddleware.cs`
- **功能**:
  - 統一例外捕捉
  - 結構化日誌記錄
  - JSON 錯誤回應格式化
  - 特定例外型別映射 (400, 401, 404, 500)

## 資料模型定義

### 1. User (使用者)
```csharp
- Id: int (主鍵)
- AdAccount: string (唯一, Active Directory 帳號)
- Name: string (使用者全名)
- Email: string (唯一, 電子郵件)
- Role: string (applicant/reviewer)
- IsActive: bool (帳號啟用狀態)
- LastLoginAt: DateTime? (最後登入)
- CreatedAt, UpdatedAt: DateTime
```

### 2. Appointment (預約)
```csharp
- Id: int (主鍵)
- ApplicantId: int (申請人外鍵)
- ReviewerId: int (審查人員外鍵)
- Date: DateTime (預約日期)
- TimeStart, TimeEnd: TimeSpan (時段)
- ObjectName: string (契約物件名稱)
- Status: string (pending/accepted/rejected/delegated/cancelled)
- DelegateReviewerId: int? (代理審查人員)
- DelegateStatus: string? (轉送狀態)
- CreatedById, CancelledAt, CancelledReason
```

### 3. LeaveSchedule (休假排程)
```csharp
- Id: int (主鍵)
- ReviewerId: int (審查人員外鍵)
- Date: DateTime (休假日期)
- TimeStart, TimeEnd: TimeSpan (休假時段)
- CreatedAt, UpdatedAt: DateTime
```

### 4. AppointmentHistory (預約歷史)
```csharp
- Id: int (主鍵)
- AppointmentId: int (預約外鍵)
- Action: string (操作類型)
- ActorId: int (操作者外鍵)
- Timestamp: DateTime (操作時間)
- Notes: string? (備註)
```

### 5. NotificationLog (通知日誌)
```csharp
- Id: int (主鍵)
- AppointmentId: int (預約外鍵)
- RecipientEmail: string (收件人)
- NotificationType: string (通知類型)
- Subject, Content: string (郵件主旨和內容)
- Status: string (pending/sent/failed)
- RetryCount: int (重試次數)
- SentAt, CreatedAt, UpdatedAt: DateTime
- ErrorMessage: string? (錯誤訊息)
```

## 專案構建狀態

✅ **編譯**: 成功 (0 錯誤)  
✅ **程式碼品質**: 無警告  
✅ **可執行性**: 就緒  
✅ **依賴解析**: 完全  

## 檔案清單

已建立的檔案:
- `backend/Models/Domain/User.cs`
- `backend/Models/Domain/Appointment.cs`
- `backend/Models/Domain/LeaveSchedule.cs`
- `backend/Models/Domain/AppointmentHistory.cs`
- `backend/Models/Domain/NotificationLog.cs`
- `backend/Data/ApplicationDbContext.cs`
- `backend/Middleware/ExceptionHandlingMiddleware.cs`
- `backend/Program.cs` (已更新)
- `backend/appsettings.Development.json` (已更新)
- `backend/ContractReviewScheduler.csproj` (已更新)

已建立的目錄:
- `backend/Controllers/`
- `backend/Services/`
- `backend/HostedServices/`
- `backend/Templates/`
- `backend/Tests/Unit/`
- `backend/Tests/Integration/`
- `backend/Tests/Contract/`

## Phase 2 (基礎設施) - 待實施

### 5 項認證基礎設施任務:

- [ ] T008: LDAP 認證服務 (System.DirectoryServices)
  - 整合 Active Directory
  - 使用者驗證邏輯
  - AD 帳號查詢

- [ ] T009: IMemoryCache 快取層
  - AD 使用者快取 (TTL: 1 小時)
  - 審查人員清單快取
  - 快取失效管理

- [ ] T010: JWT Token 簽發與驗證
  - Token 生成
  - 簽名驗證
  - 過期管理

- [ ] T011: RBAC 授權中間件
  - 角色型存取控制
  - 端點授權
  - 權限驗證

- [ ] T012: 使用者同步服務
  - 定期 AD 同步
  - 新使用者自動建立
  - 角色更新

## 下一步行動

### 1. 完成資料庫遷移
```powershell
cd backend
dotnet ef database update
```

### 2. 啟動應用程式
```powershell
dotnet run
# API 將在 https://localhost:5001 啟動
# Swagger UI: https://localhost:5001/swagger
```

### 3. 開始 Phase 2
- 實施 LDAP 整合服務
- 配置 JWT 認證
- 實施授權中間件

## 進度統計

| 階段 | 完成 | 總數 | 狀態 |
|------|------|------|------|
| Phase 1 (設置) | 7 | 7 | ✅ 完成 |
| Phase 2 (基礎設施) | 0 | 5 | ⏳ 待開始 |
| Phase 3 (US1 預約) | 0 | 13 | ⏳ 待開始 |
| Phase 4 (US2 管理) | 0 | 8 | ⏳ 待開始 |
| Phase 5 (US3 休假) | 0 | 7 | ⏳ 待開始 |
| Phase 6 (US4 轉送) | 0 | 10 | ⏳ 待開始 |
| Phase 7 (測試) | 0 | 9 | ⏳ 待開始 |
| Phase 8 (文件) | 0 | 6 | ⏳ 待開始 |
| **總計** | **7** | **78** | **9%** |

## 技術架構

### 後端
- **框架**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0 (Code First)
- **資料庫**: SQL Server 2019+
- **認證**: LDAP + JWT
- **日誌**: Serilog (結構化日誌)
- **快取**: IMemoryCache (無 Redis)

### 前端 (待實施)
- **框架**: React 19.2.0
- **語言**: TypeScript 5.6
- **UI 庫**: Bootstrap 5.3
- **路由**: React Router
- **狀態管理**: Context API
- **HTTP 客戶端**: Axios

## 備註

- 所有文件使用繁體中文註釋
- 遵循 ASP.NET Core 最佳實踐
- SOLID 原則設計
- 為單元測試準備結構化

---

**最後更新**: 2025-11-18  
**下一個里程碑**: Phase 2 (基礎設施) 完成
