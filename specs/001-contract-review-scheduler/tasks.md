# 契約審查預約系統 - 任務清單 (Phase 2)

**日期**: 2025-11-18  
**狀態**: Phase 2 任務分解  
**技術**: ASP.NET Core 8.0 + SQL Server + EF Core  

---

## 任務分類與優先級

### 🔴 優先級 1: 關鍵基礎設施 (必須先完成)

#### Task 1.1: ASP.NET Core 專案初始化
**描述**: 建立 ASP.NET Core 8.0 Web API 專案結構  
**工作項**:
- [ ] 使用 `dotnet new webapi` 建立專案
- [ ] 配置 NuGet 套件依賴
  - EntityFrameworkCore (8.0.x)
  - EntityFrameworkCore.SqlServer
  - Serilog + Serilog.AspNetCore
  - System.DirectoryServices
  - System.IdentityModel.Tokens.Jwt
  - Microsoft.AspNetCore.Authentication.JwtBearer
- [ ] 建立標準資料夾結構 (`Models/`, `Controllers/`, `Services/`, `Data/`)
- [ ] 設定專案檔 (.csproj)

**驗收準則**:
- ✅ 專案能成功編譯
- ✅ 所有依賴套件已安裝
- ✅ 資料夾結構已建立
- ✅ 預設 Swagger 可運行

**預估工時**: 2 小時  
**相關檔案**: 無依賴

---

#### Task 1.2: SQL Server 連線設定
**描述**: 設定 SQL Server 連線和 DbContext  
**工作項**:
- [ ] 安裝 SQL Server Express 或連接到公司伺服器
- [ ] 在 `appsettings.json` 配置連線字串
- [ ] 建立 `ApplicationDbContext` (已在 `data-model.md` 中定義)
- [ ] 在 `Program.cs` 註冊 DbContext
- [ ] 配置依賴注入容器

**驗收準則**:
- ✅ DbContext 可成功連接到 SQL Server
- ✅ 連線字串在 appsettings.json 中
- ✅ DI 容器已配置

**預估工時**: 1.5 小時  
**相關檔案**: `data-model.md` (DbContext 定義)

---

#### Task 1.3: EF Core 初始遷移和資料庫建立
**描述**: 建立資料庫和表格  
**工作項**:
- [ ] 執行 `dotnet ef migrations add InitialCreate`
- [ ] 檢視生成的遷移檔案
- [ ] 執行 `dotnet ef database update`
- [ ] 驗證表格在 SQL Server 中建立

**驗收準則**:
- ✅ 5 個表格已在 SQL Server 中建立
  - users
  - appointments
  - leave_schedules
  - appointment_history
  - notification_logs
- ✅ 所有索引已建立
- ✅ 外鍵關係正確

**預估工時**: 1 小時  
**相依任務**: Task 1.2

---

#### Task 1.4: Serilog 結構化日誌設定
**描述**: 配置結構化日誌系統  
**工作項**:
- [ ] 安裝 Serilog 相關套件
  - Serilog.AspNetCore
  - Serilog.Sinks.Console
  - Serilog.Sinks.File
  - Serilog.Sinks.MSSqlServer (可選)
- [ ] 在 `Program.cs` 配置 Serilog
- [ ] 建立日誌配置檔案
- [ ] 設置日誌級別 (Development: Debug, Production: Information)

**驗收準則**:
- ✅ 應用啟動時能看到結構化日誌
- ✅ 日誌寫入檔案
- ✅ 包含時間戳、級別、訊息

**預估工時**: 1.5 小時  
**相關檔案**: 無

---

#### Task 1.5: 全域例外處理中間件
**描述**: 實現統一的錯誤處理  
**工作項**:
- [ ] 建立自訂例外類別
  - `BusinessException`
  - `ValidationException`
  - `NotFoundException`
  - `UnauthorizedException`
- [ ] 建立例外處理中間件
- [ ] 在 `Program.cs` 註冊中間件
- [ ] 定義標準錯誤回應格式

**標準錯誤回應格式**:
```csharp
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "驗證失敗",
    "details": [
      { "field": "email", "message": "電子郵件格式不正確" }
    ]
  },
  "timestamp": "2025-11-18T10:30:00Z"
}
```

**驗收準則**:
- ✅ 所有未捕獲例外回傳 500 錯誤
- ✅ 驗證失敗回傳 400 + 詳細訊息
- ✅ 未授權回傳 401
- ✅ 404 錯誤正確回傳

**預估工時**: 2 小時  
**相依任務**: Task 1.4

---

### 🟠 優先級 2: 認證與授權

#### Task 2.1: System.DirectoryServices LDAP 整合
**描述**: 實現 AD/LDAP 使用者驗證  
**工作項**:
- [ ] 建立 `LdapService` 類別
- [ ] 實現 `AuthenticateUserAsync(adAccount, password)` 方法
- [ ] 實現 `GetUserDetailsAsync(adAccount)` 方法
- [ ] 處理 LDAP 例外 (無效憑證、伺服器無法連接等)
- [ ] 編寫單元測試

**LDAP 查詢範例**:
```csharp
// 建立 DirectoryEntry
var entry = new DirectoryEntry($"LDAP://company.ad.net");
var searcher = new DirectorySearcher(entry);
searcher.Filter = $"(&(objectClass=user)(sAMAccountName={adAccount}))";
searcher.PropertiesToLoad.AddRange(new[] { "mail", "displayName", "memberOf" });
```

**驗收準則**:
- ✅ 正確 AD 帳號和密碼可通過驗證
- ✅ 錯誤的憑證被拒絕
- ✅ 伺服器連接失敗能優雅處理
- ✅ 至少 80% 程式碼涵蓋率

**預估工時**: 3 小時  
**相依任務**: Task 1.4

---

#### Task 2.2: IMemoryCache LDAP 快取層
**描述**: 實現 LDAP 驗證結果快取以提高效能  
**工作項**:
- [ ] 建立 `CachedLdapService` 裝飾器
- [ ] 配置 IMemoryCache (TTL: 1 小時)
- [ ] 實現快取失效機制
- [ ] 編寫快取測試

**快取策略**:
- 成功驗證結果快取 1 小時
- 失敗驗證不快取
- 帳號資訊快取 1 小時

**驗收準則**:
- ✅ 第一次驗證查詢 AD
- ✅ 快取中存在時不查詢 AD
- ✅ 快取 1 小時後過期

**預估工時**: 2 小時  
**相依任務**: Task 2.1

---

#### Task 2.3: JWT Token 簽發和驗證
**描述**: 實現無狀態認證  
**工作項**:
- [ ] 建立 `TokenService` 類別
- [ ] 實現 `GenerateTokenAsync(user)` 方法
  - Payload: id, adAccount, email, role, expires
  - 簽名: HS256 (對稱祕鑰)
  - TTL: 1 小時
- [ ] 實現 Token 驗證
- [ ] 在 `Program.cs` 配置 JWT 驗證
- [ ] 編寫單元測試

**Token Payload 範例**:
```json
{
  "sub": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "adAccount": "user123",
  "email": "user123@isn.co.jp",
  "role": "reviewer",
  "iat": 1637235600,
  "exp": 1637239200
}
```

**驗收準則**:
- ✅ 新登入使用者取得有效 Token
- ✅ 過期 Token 被拒絕
- ✅ 篡改的 Token 被拒絕
- ✅ Token 可正確解析

**預估工時**: 2.5 小時  
**相依任務**: Task 1.4

---

#### Task 2.4: 角色型存取控制 (RBAC)
**描述**: 實現基於角色的權限檢查  
**工作項**:
- [ ] 定義角色常數
  - `ROLE_APPLICANT`: 申請人
  - `ROLE_REVIEWER`: 審查人員
- [ ] 建立 `[Authorize]` 和 `[Authorize(Roles = "reviewer")]` 屬性
- [ ] 在控制器方法上標記角色需求
- [ ] 編寫授權原則測試

**角色權限矩陣**:
```
端點                    | 申請人 | 審查人員
/auth/login             | ✅   | ✅
/appointments/create    | ✅   | ❌
/appointments/:id/accept| ❌   | ✅
/leave-schedules/*      | ❌   | ✅
```

**驗收準則**:
- ✅ 申請人無法訪問審查人員端點
- ✅ 審查人員無法建立預約
- ✅ 未認證使用者返回 401

**預估工時**: 1.5 小時  
**相依任務**: Task 2.3

---

#### Task 2.5: 使用者同步和管理服務
**描述**: 實現使用者帳號同步和快速查詢  
**工作項**:
- [ ] 建立 `UserService` 類別
- [ ] 實現 `SyncUserFromLdapAsync(adAccount)` - LDAP 同步到資料庫
- [ ] 實現 `GetUserByAdAccountAsync(adAccount)` - 快速查詢
- [ ] 實現 `GetReviewersAsync()` - 快取審查人員清單
- [ ] 處理新使用者建立和現有使用者更新
- [ ] 編寫整合測試

**驗收準則**:
- ✅ LDAP 新使用者可自動同步到 DB
- ✅ 使用者資訊可更新
- ✅ 審查人員清單快取 1 小時

**預估工時**: 2.5 小時  
**相依任務**: Task 2.1, Task 1.3

---

### 🟡 優先級 3: 預約核心功能

#### Task 3.1: Appointment 實體和資料存取層
**描述**: 實現預約資料管理  
**工作項**:
- [ ] 建立 `IAppointmentRepository` 介面
- [ ] 實現 `AppointmentRepository` (EF Core)
- [ ] 實現查詢方法
  - `GetByIdAsync(id)` - 單筆預約
  - `GetByReviewerAsync(reviewerId, date)` - 審查人員月曆
  - `GetByApplicantAsync(applicantId)` - 申請人歷史
  - `GetConflictingAsync(reviewerId, date, timeStart, timeEnd)` - 時段衝突
- [ ] 編寫查詢效能測試

**驗收準則**:
- ✅ 所有查詢方法實現
- ✅ 複合索引加速查詢
- ✅ 查詢效能 < 50ms (在 1 萬筆預約下)

**預估工時**: 2 小時  
**相依任務**: Task 1.3

---

#### Task 3.2: 時段衝突驗證
**描述**: 實現預約時間衝突偵測  
**工作項**:
- [ ] 建立 `ConflictDetectionService` 類別
- [ ] 實現衝突檢測演算法
  - SQL Server DATEDIFF 查詢
  - 考慮 LeaveSchedule
  - 排除已拒絕/已取消預約
- [ ] 建立單元測試覆蓋各種衝突場景
- [ ] 效能測試 (1 萬筆預約 < 10ms)

**SQL 衝突偵測查詢範例**:
```sql
SELECT COUNT(*) FROM appointments
WHERE reviewer_id = @reviewerId
  AND date = @date
  AND status NOT IN ('rejected', 'cancelled')
  AND (
    (@timeStart < time_end AND @timeEnd > time_start)
    OR
    EXISTS (
      SELECT 1 FROM leave_schedules
      WHERE reviewer_id = @reviewerId
        AND date = @date
        AND (@timeStart < time_end AND @timeEnd > time_start)
    )
  )
```

**驗收準則**:
- ✅ 重疊時段被正確檢測
- ✅ 不重疊時段不被標記為衝突
- ✅ 15 分鐘邊界正確處理
- ✅ 查詢 < 10ms

**預估工時**: 3 小時  
**相依任務**: Task 1.4, Task 3.1

---

#### Task 3.3: 預約驗證規則
**描述**: 實現預約建立/修改的業務規則  
**工作項**:
- [ ] 建立 `AppointmentValidator` 類別
- [ ] 驗證規則
  - time_start < time_end
  - 時段為 15 分鐘倍數
  - 日期為工作日 (一~五)
  - 時間在營業時間內 (09:00~18:00)
  - object_name 非空 (1-500 字)
  - applicant_id ≠ reviewer_id
  - 預約 24 小時後才能修改/取消
- [ ] 使用 FluentValidation (可選)
- [ ] 編寫全面測試

**驗收準則**:
- ✅ 所有規則已驗證
- ✅ 驗證失敗回傳有意義的錯誤訊息
- ✅ 驗證 100% 涵蓋率

**預估工時**: 2.5 小時  
**相依任務**: Task 1.4

---

#### Task 3.4: Appointment 業務邏輯服務
**描述**: 實現預約的 CRUD 和狀態管理  
**工作項**:
- [ ] 建立 `AppointmentService` 類別
- [ ] 實現 `CreateAppointmentAsync(dto)` - 建立預約
  - 驗證輸入
  - 檢測衝突
  - 保存到 DB
  - 建立歷史記錄
  - 觸發通知
- [ ] 實現 `UpdateAppointmentAsync(id, dto)` - 更新
- [ ] 實現 `CancelAppointmentAsync(id, reason)` - 取消
- [ ] 實現 `GetAppointmentAsync(id)` - 查詢單筆
- [ ] 實現 `ListAppointmentsAsync(filters)` - 列表查詢
- [ ] 編寫整合測試

**驗收準則**:
- ✅ 預約可成功建立、更新、取消
- ✅ 衝突預約被拒絕
- ✅ 24 小時檢查運作正確
- ✅ 歷史記錄自動建立
- ✅ 70% 程式碼涵蓋率

**預估工時**: 3 小時  
**相依任務**: Task 3.1, Task 3.2, Task 3.3

---

#### Task 3.5: Appointment 控制器
**描述**: 實現預約 REST API 端點  
**工作項**:
- [ ] 建立 `AppointmentsController` 類別
- [ ] 實現端點 (參考 `contracts/openapi.yaml`)
  - `POST /api/appointments` - 建立
  - `GET /api/appointments/:id` - 查詢
  - `GET /api/appointments` - 列表
  - `PUT /api/appointments/:id` - 更新
  - `DELETE /api/appointments/:id` - 取消
- [ ] 加上 `[Authorize]` 屬性
- [ ] DTO 物件和模型對應
- [ ] 編寫端點測試

**驗收準則**:
- ✅ 所有 5 個端點實現
- ✅ 正確的 HTTP 狀態碼 (201, 200, 400, 401, 404)
- ✅ 端點測試通過

**預估工時**: 2 小時  
**相依任務**: Task 3.4

---

### 🟡 優先級 4: 審查人員功能

#### Task 4.1: LeaveSchedule 資料層和業務邏輯
**描述**: 實現休假排程管理  
**工作項**:
- [ ] 建立 `ILeaveScheduleRepository` 介面
- [ ] 實現 `LeaveScheduleRepository`
- [ ] 建立 `LeaveScheduleService` 類別
- [ ] 實現 `CreateLeaveAsync(reviewerId, date, timeStart, timeEnd)`
  - 檢查該時段無預約
  - 檢查無重複休假
- [ ] 實現 `DeleteLeaveAsync(id)` - 刪除休假
- [ ] 實現 `GetLeavesAsync(reviewerId, month)` - 查詢
- [ ] 編寫整合測試

**驗收準則**:
- ✅ 休假可建立和刪除
- ✅ 無法在有預約時段建立休假
- ✅ 無法建立重複休假
- ✅ 查詢運作正確

**預估工時**: 2.5 小時  
**相依任務**: Task 1.3, Task 3.1

---

#### Task 4.2: LeaveSchedule 控制器
**描述**: 實現休假 REST API  
**工作項**:
- [ ] 建立 `LeaveSchedulesController`
- [ ] 實現端點
  - `POST /api/leave-schedules` - 新建
  - `GET /api/leave-schedules` - 列表
  - `DELETE /api/leave-schedules/:id` - 刪除
- [ ] 限制只有 reviewer 角色能存取
- [ ] 編寫端點測試

**驗收準則**:
- ✅ 只有 reviewer 能建立休假
- ✅ 申請人無法存取
- ✅ 端點測試通過

**預估工時**: 1 小時  
**相依任務**: Task 4.1

---

#### Task 4.3: 預約確認流程 (Accept/Reject)
**描述**: 實現審查人員接受/拒絕預約  
**工作項**:
- [ ] 在 AppointmentService 新增方法
  - `AcceptAppointmentAsync(appointmentId, reviewerId)` - 接受
  - `RejectAppointmentAsync(appointmentId, reviewerId, reason)` - 拒絕
- [ ] 狀態轉換檢查 (只有 pending 能轉換)
- [ ] 建立歷史記錄
- [ ] 觸發郵件通知
- [ ] 編寫整合測試

**驗收準則**:
- ✅ 預約可被接受/拒絕
- ✅ 只有指定審查人員能確認
- ✅ 狀態正確更新
- ✅ 通知被觸發

**預估工時**: 2 小時  
**相依任務**: Task 3.4

---

#### Task 4.4: 預約轉送和代理接受
**描述**: 實現預約轉送給代理人  
**工作項**:
- [ ] 在 AppointmentService 新增方法
  - `DelegateAppointmentAsync(appointmentId, delegateReviewerId)` - 轉送
  - `AcceptDelegationAsync(appointmentId, delegateReviewerId)` - 代理接受
  - `RejectDelegationAsync(appointmentId, reason)` - 代理拒絕
- [ ] 驗證代理人可用性 (無衝突、無休假)
- [ ] 狀態轉換: pending → delegated → delegate_accepted/rejected
- [ ] 建立歷史記錄和通知
- [ ] 編寫整合測試

**驗收準則**:
- ✅ 預約可轉送給代理人
- ✅ 代理人可接受或拒絕
- ✅ 狀態轉換正確
- ✅ 通知被發送

**預估工時**: 2.5 小時  
**相依任務**: Task 4.3

---

#### Task 4.5: Appointment 確認和轉送控制器
**描述**: 實現確認/轉送的 API 端點  
**工作項**:
- [ ] 在 AppointmentsController 新增端點
  - `POST /api/appointments/:id/accept` - 接受
  - `POST /api/appointments/:id/reject` - 拒絕
  - `POST /api/appointments/:id/delegate` - 轉送
  - `POST /api/appointments/:id/accept-delegation` - 代理接受
  - `POST /api/appointments/:id/reject-delegation` - 代理拒絕
- [ ] 限制角色 (只有 reviewer)
- [ ] 編寫端點測試

**驗收準則**:
- ✅ 所有 5 個端點實現
- ✅ 只有審查人員能確認
- ✅ 端點測試通過

**預估工時**: 1.5 小時  
**相依任務**: Task 4.3, Task 4.4

---

### 🟡 優先級 5: 郵件通知系統

#### Task 5.1: NotificationLog 資料層
**描述**: 實現通知紀錄管理  
**工作項**:
- [ ] 建立 `INotificationLogRepository` 介面
- [ ] 實現 `NotificationLogRepository`
- [ ] 實現方法
  - `CreateNotificationAsync(dto)` - 建立通知紀錄
  - `UpdateStatusAsync(id, status, sentAt)` - 更新狀態
  - `IncrementRetryAsync(id, errorMessage)` - 重試計數

**驗收準則**:
- ✅ 通知紀錄可建立和更新
- ✅ 失敗通知可記錄錯誤訊息

**預估工時**: 1 小時  
**相依任務**: Task 1.3

---

#### Task 5.2: 郵件服務實現
**描述**: 實現 System.Net.Mail SMTP 郵件發送  
**工作項**:
- [ ] 建立 `EmailService` 類別
- [ ] 實現 `SendEmailAsync(to, subject, body)` 方法
- [ ] 配置 SMTP 伺服器設定 (appsettings.json)
  - Host: company-smtp.com (或公司設定)
  - Port: 587
  - EnableSSL: true
- [ ] 例外處理和日誌
- [ ] 編寫單元測試 (模擬 SMTP)

**SMTP 設定範例**:
```json
"Email": {
  "SmtpServer": "mail.isn.co.jp",
  "SmtpPort": 587,
  "FromAddress": "noreply@isn.co.jp",
  "FromName": "契約審查系統",
  "EnableSsl": true
}
```

**驗收準則**:
- ✅ 郵件可成功發送
- ✅ 例外被正確處理
- ✅ SMTP 連接失敗能優雅降級

**預估工時**: 2 小時  
**相依任務**: Task 1.4

---

#### Task 5.3: 通知範本管理
**描述**: 建立和管理郵件通知範本  
**工作項**:
- [ ] 建立通知範本類別 (8 種類型)
  - appointment_created - 新預約
  - appointment_accepted - 已接受
  - appointment_rejected - 已拒絕
  - appointment_delegated - 已轉送
  - delegate_accepted - 代理接受
  - delegate_rejected - 代理拒絕
  - appointment_cancelled - 已取消
  - appointment_modified - 已修改
- [ ] 每個範本包含 Subject 和 Body (HTML)
- [ ] 使用佔位符進行客製化 ({{appointmentId}}, {{reviewerName}} 等)
- [ ] 編寫範本測試

**範本存儲位置**: `Services/NotificationTemplates/` 或資料庫

**驗收準則**:
- ✅ 所有 8 種通知類型有對應範本
- ✅ 佔位符被正確替換
- ✅ HTML 格式正確

**預估工時**: 1.5 小時  
**相依任務**: Task 5.1

---

#### Task 5.4: IHostedService 郵件佇列
**描述**: 實現後台郵件發送服務  
**工作項**:
- [ ] 建立 `NotificationQueueService` 實現 `IHostedService`
- [ ] 定期檢查待發送通知 (每分鐘)
  - 查詢 status='pending' 的通知
  - 嘗試發送
  - 失敗時記錄錯誤
  - 重試 3 次
- [ ] 在 `Program.cs` 註冊服務
- [ ] 編寫整合測試

**重試機制**:
- 第 1 次: 立即重試
- 第 2 次: 5 分鐘後
- 第 3 次: 15 分鐘後
- 3 次失敗後標記為 failed

**驗收準則**:
- ✅ 待發送通知每分鐘檢查一次
- ✅ 成功發送更新 status='sent' 和 sent_at
- ✅ 失敗通知重試 3 次
- ✅ 3 次失敗後標記 failed

**預估工時**: 2.5 小時  
**相依任務**: Task 5.1, Task 5.2

---

#### Task 5.5: 通知發送整合點
**描述**: 在業務邏輯中觸發通知  
**工作項**:
- [ ] 在 AppointmentService 新增方法
  - `EnqueueNotificationAsync(appointmentId, notificationType, recipients)`
- [ ] 在以下事件觸發通知
  - 建立預約 → 通知審查人員
  - 接受預約 → 通知申請人
  - 拒絕預約 → 通知申請人
  - 轉送預約 → 通知代理人
  - 代理接受 → 通知申請人和原審查人員
  - 代理拒絕 → 通知申請人和原審查人員
  - 取消預約 → 通知審查人員
- [ ] 編寫整合測試

**驗收準則**:
- ✅ 所有事件觸發相應通知
- ✅ 通知記錄在資料庫中
- ✅ 整合測試通過

**預估工時**: 2 小時  
**相依任務**: Task 5.4, Task 3.4

---

### 🟢 優先級 6: 測試和品質保證

#### Task 6.1: 單元測試基礎設施
**描述**: 建立單元測試框架和工具  
**工作項**:
- [ ] 建立測試專案 `*.Tests`
- [ ] 安裝測試框架
  - xUnit
  - Moq (模擬)
  - FluentAssertions
- [ ] 建立基礎測試類別和助手方法
- [ ] 設置測試執行器 (xUnit runner)

**驗收準則**:
- ✅ 測試專案能編譯和執行
- ✅ 基礎設施就位

**預估工時**: 1.5 小時  
**相依任務**: Task 1.1

---

#### Task 6.2: 業務邏輯單元測試
**描述**: 測試所有 Service 類別  
**工作項**:
- [ ] ConflictDetectionService 測試 (10+ 案例)
- [ ] AppointmentValidator 測試 (15+ 案例)
- [ ] UserService 測試 (5+ 案例)
- [ ] TokenService 測試 (5+ 案例)
- [ ] 目標涵蓋率: 80%

**測試案例範例 (ConflictDetectionService)**:
- 無衝突時段
- 重疊時段
- 鄰接時段 (15 分鐘邊界)
- 休假衝突
- 多個重疊預約

**驗收準則**:
- ✅ 所有 Service 類別有單元測試
- ✅ 涵蓋率 ≥ 80%
- ✅ 測試通過

**預估工時**: 5 小時  
**相依任務**: Task 6.1, Task 3.2, Task 3.3, Task 2.3, Task 2.5

---

#### Task 6.3: 資料層整合測試
**描述**: 測試 EF Core 和資料庫互動  
**工作項**:
- [ ] 建立測試用 in-memory 或 LocalDB
- [ ] 測試 Repository 方法 (CRUD)
- [ ] 測試複雜查詢 (衝突偵測、月曆查詢)
- [ ] 測試外鍵約束
- [ ] 涵蓋率: 75%+

**驗收準則**:
- ✅ 所有 Repository 方法有測試
- ✅ 複雜查詢被驗證
- ✅ 測試通過

**預估工時**: 4 小時  
**相依任務**: Task 1.3

---

#### Task 6.4: API 端點整合測試
**描述**: 測試 HTTP 端點  
**工作項**:
- [ ] 使用 `WebApplicationFactory` 建立測試伺服器
- [ ] 測試所有 CRUD 端點
- [ ] 測試認證 (JWT 令牌)
- [ ] 測試授權 (角色檢查)
- [ ] 測試錯誤案例 (400, 401, 404, 500)
- [ ] 涵蓋率: 70%+

**測試端點範例**:
- `POST /api/appointments` - 成功 + 驗證失敗 + 無授權
- `GET /api/appointments/:id` - 存在 + 不存在
- `PUT /api/appointments/:id` - 成功 + 24小時檢查

**驗收準則**:
- ✅ 所有控制器方法有測試
- ✅ 認證和授權被驗證
- ✅ 測試通過

**預估工時**: 4 小時  
**相依任務**: Task 3.5, Task 4.2, Task 4.5, Task 2.3, Task 2.4

---

#### Task 6.5: 效能測試
**描述**: 驗證效能指標  
**工作項**:
- [ ] 測試 API 回應時間 (目標 < 200ms)
  - 單個預約查詢
  - 列表查詢 (1000 筆預約)
  - 衝突偵測 (1 萬筆預約)
- [ ] 測試資料庫查詢時間
- [ ] 使用壓力測試工具 (NBench 或 BenchmarkDotNet)

**效能基準**:
- 單筆查詢 < 10ms
- 列表查詢 < 50ms
- 衝突偵測 < 20ms

**驗收準則**:
- ✅ 所有 API 端點 < 200ms (95th percentile)
- ✅ 資料庫查詢優化
- ✅ 效能報告文件

**預估工時**: 3 小時  
**相依任務**: Task 3.5, Task 4.2

---

#### Task 6.6: 程式碼品質檢查
**描述**: 進行程式碼分析和品質檢查  
**工作項**:
- [ ] 安裝 StyleCop.Analyzers (程式碼風格)
- [ ] 執行 SonarQube 或 CodeQL 分析
- [ ] 檢查循環複雜度 (目標 < 10)
- [ ] 檢查程式碼涵蓋率報告
- [ ] 修復所有警告和問題

**驗收準則**:
- ✅ 無 StyleCop 警告 (或文件化例外)
- ✅ 循環複雜度 < 10
- ✅ 程式碼涵蓋率 ≥ 80%
- ✅ 無安全漏洞警告

**預估工時**: 2 小時  
**相依任務**: Task 6.2, Task 6.3, Task 6.4

---

### 🟢 優先級 7: 文件和部署

#### Task 7.1: API 文件補充
**描述**: 完善 Swagger/OpenAPI 文件  
**工作項**:
- [ ] 配置 Swagger UI 在 `/swagger/index.html`
- [ ] 新增 XML 註釋到所有控制器和方法
- [ ] 補充端點描述、參數、回應範例
- [ ] 新增認證說明 (JWT 令牌)
- [ ] 新增錯誤碼說明

**Swagger 配置範例**:
```csharp
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "契約審查預約系統 API", 
        Version = "v1" 
    });
    c.AddSecurityDefinition("Bearer", ...);
    c.IncludeXmlComments(xmlPath);
});
```

**驗收準則**:
- ✅ Swagger UI 能正常顯示
- ✅ 所有端點有詳細文件
- ✅ 範例請求/回應可測試

**預估工時**: 2 小時  
**相依任務**: Task 3.5, Task 4.2, Task 4.5

---

#### Task 7.2: 開發者操作手冊
**描述**: 編寫營運指南  
**工作項**:
- [ ] 建立 `OPERATIONS.md` 檔案
- [ ] 內容
  - 部署步驟 (本機、測試環境、生產環境)
  - 資料庫遷移流程
  - 常見問題排查
  - 監控和告警設定
  - 備份和恢復程序
  - 安全檢查清單

**驗收準則**:
- ✅ 文件清晰詳細
- ✅ 包含所有必要步驟
- ✅ 中文撰寫 (zh-TW)

**預估工時**: 2 小時  
**相依任務**: 無

---

#### Task 7.3: 部署和 CI/CD 設置 (可選)
**描述**: 設置持續整合和部署  
**工作項**:
- [ ] 建立 GitHub Actions 工作流程 (可選)
  - 編譯檢查
  - 執行測試
  - 程式碼品質檢查
  - 發布到 NuGet (可選)
- [ ] 或設置公司 CI/CD 伺服器 (如 Azure DevOps)
- [ ] 建立 Docker 映像 (可選)

**可選部分**: 根據公司基礎設施

**驗收準則**:
- ✅ CI/CD 管線正常運行
- ✅ 測試在每次提交時執行

**預估工時**: 3 小時 (可選)  
**相依任務**: Task 6.1, Task 6.4

---

## 時間和資源規劃

### 工時總結

| 優先級 | 領域 | 任務數 | 預估時數 |
|--------|------|--------|---------|
| 1 | 基礎設施 | 5 | 7.5 小時 |
| 2 | 認證授權 | 5 | 12 小時 |
| 3 | 預約核心 | 5 | 12 小時 |
| 4 | 審查人員 | 5 | 9 小時 |
| 5 | 郵件系統 | 5 | 9 小時 |
| 6 | 測試 QA | 6 | 19 小時 |
| 7 | 文件部署 | 3 | 7 小時 |
| **總計** | | **33 個任務** | **~75 小時** |

### 建議實施順序

1. **第 1 周**: 優先級 1 (基礎設施) - 7.5 小時
2. **第 2 周**: 優先級 2 (認證授權) - 12 小時
3. **第 3-4 周**: 優先級 3 (預約核心) + 平行優先級 5 (郵件) - 12+9=21 小時
4. **第 4-5 周**: 優先級 4 (審查人員) - 9 小時
5. **第 5-6 周**: 優先級 6 (測試 QA) - 19 小時
6. **第 6 周**: 優先級 7 (文件部署) - 7 小時

**總預計**: 6 周 (1 名開發者全職)

### 資源需求

- **開發者**: 1 名 (ASP.NET Core + SQL Server)
- **QA**: 可包含開發者或獨立
- **基礎設施**: SQL Server、SMTP、AD 伺服器存取

---

## 檢查清單範本

使用以下清單追蹤任務進度：

```markdown
- [ ] Task 1.1: ASP.NET Core 專案初始化
- [ ] Task 1.2: SQL Server 連線設定
- [ ] Task 1.3: EF Core 初始遷移
- [ ] Task 1.4: Serilog 日誌設定
- [ ] Task 1.5: 全域例外處理中間件
- [ ] Task 2.1: LDAP 整合
- [ ] Task 2.2: IMemoryCache 快取層
- [ ] Task 2.3: JWT Token 簽發
- [ ] Task 2.4: RBAC 實現
- [ ] Task 2.5: 使用者同步服務
... (依此類推)
```

---

**文件更新日期**: 2025-11-18  
**狀態**: Phase 2 任務分解完成，準備開始實施
