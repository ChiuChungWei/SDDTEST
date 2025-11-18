# 🚀 快速參考卡 - 契約審查預約系統

## 📍 核心資訊

| 項目 | 內容 |
|------|------|
| **系統名稱** | 契約審查預約系統 |
| **英文名** | Contract Review Appointment System |
| **技術棧** | ASP.NET Core 8.0 + SQL Server + EF Core |
| **狀態** | ✅ Phase 0-1 規劃完成 |
| **分支** | `001-contract-review-scheduler` |
| **開始日期** | 2025-11-18 |

---

## 📁 重要檔案位置

### 開發者必讀 (按順序)
```
1. specs/001-contract-review-scheduler/README.md           ← 從這裡開始
2. specs/001-contract-review-scheduler/quickstart.md       ← 環境設置
3. specs/001-contract-review-scheduler/spec.md             ← 功能定義
4. specs/001-contract-review-scheduler/data-model.md       ← DB 設計 + C# 程式碼
5. specs/001-contract-review-scheduler/tasks.md            ← 實施任務
```

### 建築與決策
```
specs/001-contract-review-scheduler/research.md            ← 11 項技術決策
specs/001-contract-review-scheduler/contracts/openapi.yaml ← 11 API 端點
```

### 參考資源
```
.specify/memory/constitution.md                             ← 專案憲章 v2.0.0
PLANNING_COMPLETE.md                                        ← 規劃完成報告
```

---

## 🎯 重點決策

### ✅ 必須採用
- ✅ ASP.NET Core 8.0 (不是 Node.js)
- ✅ SQL Server (不是 PostgreSQL)
- ✅ EF Core Code First (版本控制)
- ✅ POCO 物件 (不用 AutoMapper)
- ✅ IMemoryCache (不用 Redis)
- ✅ 傳統 Controllers (不用 Minimal APIs)
- ✅ System.DirectoryServices LDAP (WiAD 認證)
- ✅ System.Net.Mail + IHostedService (郵件佇列)
- ✅ JWT + LDAP 認證

### ❌ 禁止使用
- ❌ 前端 UI (REST API 只)
- ❌ AutoMapper (POCO 直接映射)
- ❌ Redis (用 IMemoryCache)
- ❌ Minimal APIs (用 Controllers)
- ❌ 其他 ORM (用 EF Core)

---

## 📋 快速檢查清單

### 開始前檢查
- [ ] 安裝 .NET 8 SDK
- [ ] 安裝 Visual Studio 2022 或 VS Code + CLI
- [ ] 安裝 SQL Server 2019+ (或 Express)
- [ ] 有公司 AD 伺服器存取權
- [ ] 有公司 SMTP 郵件伺服器存取權

### 環境設置
- [ ] 閱讀 `quickstart.md`
- [ ] 建立新 ASP.NET Core 專案
- [ ] 連接 SQL Server
- [ ] 執行初始遷移 (`dotnet ef database update`)

### 開始編碼
- [ ] 打開 `tasks.md`
- [ ] 按優先級順序實施 Task 1.1-1.5
- [ ] 每完成一個 task 在清單中打勾
- [ ] 定期推送 git 提交

---

## 📊 工作量估計

### 按優先級
| 級別 | 任務數 | 工時 | 周數 |
|------|--------|------|------|
| 🔴 1 | 5 | 7.5h | 1w |
| 🟠 2 | 5 | 12h | 2w |
| 🟡 3 | 5 | 12h | 2w |
| 🟡 4 | 5 | 9h | 1.5w |
| 🟡 5 | 5 | 9h | 1.5w |
| 🟢 6 | 6 | 19h | 3w |
| 🟢 7 | 3 | 7h | 1w |
| **總計** | **33** | **75h** | **6w** |

---

## 🛠️ 關鍵技術棧

### 後端
```
ASP.NET Core 8.0 (C#)
├─ EntityFrameworkCore 8.0
├─ EntityFrameworkCore.SqlServer
├─ System.DirectoryServices (LDAP)
├─ System.Net.Mail (SMTP)
├─ System.IdentityModel.Tokens.Jwt (JWT)
├─ Microsoft.AspNetCore.Authentication.JwtBearer
└─ Serilog (日誌)

SQL Server 2019+
└─ 5 個表格 (User, Appointment, LeaveSchedule, AppointmentHistory, NotificationLog)
```

### 測試
```
xUnit (單元測試)
├─ Moq (模擬)
└─ FluentAssertions
```

### API 文件
```
Swagger/OpenAPI 3.0
├─ 11 個 REST 端點
└─ 完整的請求/回應範例
```

---

## 🏗️ 資料模型 (5 實體)

```
User (使用者)
  ├─ id, ad_account, email, role
  └─ 1:N → Appointment, LeaveSchedule, AppointmentHistory

Appointment (預約)
  ├─ id, applicant_id, reviewer_id, date, time_start, time_end
  ├─ object_name, status
  └─ 1:N → AppointmentHistory, NotificationLog

LeaveSchedule (休假)
  ├─ id, reviewer_id, date, time_start, time_end
  └─ reason

AppointmentHistory (歷史)
  ├─ id, appointment_id, action, actor_id
  └─ old_status, new_status, timestamp, details (JSON)

NotificationLog (通知)
  ├─ id, appointment_id, recipient_id, notification_type
  └─ status, sent_at, retry_count
```

---

## 🌐 API 概覽

### 認證
```
POST   /api/auth/login              登入
GET    /api/auth/profile            個人資料
```

### 預約
```
POST   /api/appointments            建立
GET    /api/appointments            列表
GET    /api/appointments/:id        查詢
PUT    /api/appointments/:id        更新
DELETE /api/appointments/:id        取消
POST   /api/appointments/:id/accept 接受
POST   /api/appointments/:id/reject 拒絕
```

### 審查人員
```
POST   /api/appointments/:id/delegate           轉送
POST   /api/appointments/:id/accept-delegation  代理接受
POST   /api/appointments/:id/reject-delegation  代理拒絕
```

### 休假
```
POST   /api/leave-schedules        建立
GET    /api/leave-schedules        列表
DELETE /api/leave-schedules/:id    刪除
```

### 月曆
```
GET    /api/calendar/:reviewerId/month 月曆視圖
```

---

## 💾 Git 工作流程

### 提交規則
```
feat(xxx):  新功能
fix(xxx):   錯誤修正
docs(xxx):  文件更新
test(xxx):  測試代碼
refactor(xxx): 重構
```

### 範例
```
git add src/Models/Appointment.cs
git commit -m "feat(appointment): 新增 Appointment 實體和驗證規則"
git push origin 001-contract-review-scheduler
```

---

## 🔐 安全檢查清單

### 認證
- [ ] 使用 LDAP 驗證 AD 帳號
- [ ] 簽發 JWT token (1 小時 TTL)
- [ ] 拒絕過期 token

### 授權
- [ ] 申請人只能建立預約
- [ ] 審查人員只能接受/拒絕
- [ ] 驗證使用者身份和角色

### 資料驗證
- [ ] 所有輸入驗證 (類型、範圍、格式)
- [ ] 防止 SQL injection (使用參數化查詢)
- [ ] 防止時間衝突

---

## 📞 常見問題快速解答

**Q: 我該從 Task 1.1 還是其他地方開始?**  
A: 從 Task 1.1 開始。優先級已設定。

**Q: 如何處理時間衝突?**  
A: 使用 SQL Server DATEDIFF 和 IMemoryCache 快取。見 `data-model.md`。

**Q: 郵件如何發送?**  
A: 用 IHostedService 後台佇列。見 Task 5.4。

**Q: 如何快取 LDAP 結果?**  
A: 用 IMemoryCache，TTL 1 小時。見 Task 2.2。

**Q: 需要 Redis 嗎?**  
A: 不需要。IMemoryCache 足夠。

**Q: 工期能縮短嗎?**  
A: 可能，但不建議跳過測試 (Task 6)。

---

## 🎓 關鍵概念

### POCO 物件映射
```csharp
// ✅ 建議 - 直接使用模型
var appointment = new Appointment { ... };
return appointment;

// ❌ 禁止 - AutoMapper
var dto = _mapper.Map<AppointmentDto>(appointment);
```

### DbContext 配置
```csharp
// ✅ 建議 - 完整的 Fluent API 配置
modelBuilder.Entity<Appointment>(entity =>
{
    entity.HasIndex(e => new { e.ReviewerId, e.Date });
    entity.HasOne(e => e.Reviewer)
        .WithMany()
        .HasForeignKey(e => e.ReviewerId);
});
```

### JWT Token
```csharp
// ✅ 建議 - 1 小時 TTL
var token = _tokenService.GenerateToken(user);  // 自動過期
```

---

## 📖 文件清單

**核心規劃** (必讀)
- [ ] `README.md` - 導覽指南
- [ ] `spec.md` - 功能規格
- [ ] `data-model.md` - DB 設計 + C# 代碼
- [ ] `tasks.md` - 實施任務

**參考資料** (需要時查看)
- [ ] `research.md` - 技術決策理由
- [ ] `contracts/openapi.yaml` - API 規約
- [ ] `quickstart.md` - 環境設置
- [ ] `plan.md` - 進度追蹤

**驗證清單**
- [ ] `checklists/requirements.md` - 規格驗證

---

## ✨ 成功標誌

### Phase 2 起始
- ✅ 環境已設置 (Visual Studio + SQL Server)
- ✅ 專案已建立
- ✅ DbContext 已配置
- ✅ 初始遷移已執行

### 中間檢查 (3 周後)
- ✅ 認證系統完成 (Task 2)
- ✅ 預約核心完成 (Task 3)
- ✅ 基本 API 可用

### 最終檢查 (6 周後)
- ✅ 所有 33 個任務完成
- ✅ 80% 程式碼涵蓋率
- ✅ API 文件完整
- ✅ 可部署到生產環境

---

**版本**: 1.0  
**更新日期**: 2025-11-18  
**狀態**: ✅ 準備實施  
🚀 **讓我們開始編碼!**
