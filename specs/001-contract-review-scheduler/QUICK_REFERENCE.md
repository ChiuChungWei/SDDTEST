# 契約審查預約系統 - 快速參考指南 v2.0

**快速查詢**: 用於快速找到所需資訊的索引表  
**最後更新**: 2025-11-18  

---

## 📋 我需要... (快速導覽)

### 💡 了解系統功能
→ 閱讀 [`spec.md`](./spec.md) (第 1-50 行) 中的 4 個使用案例

### 🏗️ 了解整體架構
→ 查看 [`frontend-backend-integration.md`](./frontend-backend-integration.md) 中的架構概觀圖

### 👨‍💻 開始後端開發
```
1. 閱讀 [`quickstart.md`](./quickstart.md) - 環境設置
2. 執行命令設置環境
3. 查看 [`data-model.md`](./data-model.md) - 資料模型
4. 使用 [`tasks.md`](./tasks.md) 優先級 1 開始
```

### 🎨 開始前端開發
```
1. 閱讀 [`frontend-quickstart.md`](./frontend-quickstart.md) - 環境設置
2. 執行命令設置環境
3. 查看 [`frontend-components.md`](./frontend-components.md) - 元件結構
4. 使用 [`tasks.md`](./tasks.md) 優先級 0 開始
```

### 🔌 了解 API 規約
→ 查看 [`contracts/openapi.yaml`](./contracts/openapi.yaml) 或 Swagger UI

### 🔐 了解認證流程
→ 閱讀 [`frontend-backend-integration.md`](./frontend-backend-integration.md) 中的認證流程部分

### 📊 查看進度和工期
→ 查看 [`tasks.md`](./tasks.md) 中的優先級和工時估算

### 🧪 了解測試策略
→ 查看 [`tasks.md`](./tasks.md) 中優先級 6 (後端) 或優先級 B (前端)

### 🚀 部署準備
→ 查看 [`frontend-quickstart.md`](./frontend-quickstart.md) 或 [`quickstart.md`](./quickstart.md) 中的部署部分

### 👥 前後端協調
→ 閱讀 [`frontend-backend-integration.md`](./frontend-backend-integration.md) 中的開發協調部分

### 📖 查看所有文件
→ 查看 [`README.md`](./README.md) 的成品矩陣

---

## 🎯 關鍵決策快速查詢

### 後端為什麼選擇...?

| 決策 | 理由 | 文件 |
|------|------|------|
| ASP.NET Core 8.0 | 企業級框架，LDAP 支援 | [`research.md`](./research.md) #1 |
| SQL Server 2019+ | 企業級資料庫，T-SQL 支援 | [`research.md`](./research.md) #3 |
| Entity Framework Core | Code First 方式，自動遷移 | [`research.md`](./research.md) #4 |
| 無 AutoMapper，使用 POCO | 簡化映射邏輯 | [`research.md`](./research.md) #5 |
| IMemoryCache 無 Redis | 簡化部署，足夠應用層快取 | [`research.md`](./research.md) #6 |
| Traditional Controllers | RESTful 標準，易於理解 | [`research.md`](./research.md) #7 |
| LDAP + JWT | 企業認證 + 無狀態 API | [`research.md`](./research.md) #1, #8 |

### 前端為什麼選擇...?

| 決策 | 理由 | 文件 |
|------|------|------|
| React 19.2.0 | 現代 Hook 架構，活躍社群 | [`frontend-research.md`](./frontend-research.md) #1 |
| TypeScript 5.6 | 型別安全，提升開發體驗 | [`frontend-research.md`](./frontend-research.md) #2 |
| Bootstrap 5.3+ | 完整元件庫，快速原型 | [`frontend-research.md`](./frontend-research.md) #3 |
| Vite 構建工具 | 快速開發伺服器 | [`frontend-research.md`](./frontend-research.md) #4 |
| React Router v6 | 客端路由，支援嵌套路由 | [`frontend-research.md`](./frontend-research.md) #5 |
| Axios | 簡單 HTTP 客戶端，攔截器支援 | [`frontend-research.md`](./frontend-research.md) #6 |
| Context API | 輕量級狀態，無需 Redux | [`frontend-research.md`](./frontend-research.md) #7 |
| React Big Calendar | 專業日程元件 | [`frontend-research.md`](./frontend-research.md) #8 |
| Formik + Yup | 表單驗證標準方案 | [`frontend-research.md`](./frontend-research.md) #9 |
| Vitest | 現代測試框架，快速 | [`frontend-research.md`](./frontend-research.md) #10 |
| i18n-next | 國際化標準，繁體中文支援 | [`frontend-research.md`](./frontend-research.md) #12 |

---

## 📊 工作項速查表

### 後端優先級 1: 基礎設施 (7.5h)
| Task | 名稱 | 工時 | 內容 |
|------|------|------|------|
| 1.1 | 專案初始化 | 1.5h | 建立 ASP.NET Core 專案結構 |
| 1.2 | NuGet 套件 | 1h | 安裝 EF Core、Serilog 等 |
| 1.3 | DbContext 配置 | 2h | 建立 ApplicationDbContext |
| 1.4 | 資料庫遷移 | 1.5h | 執行 EF Core 遷移 |
| 1.5 | 測試連線 | 1.5h | 驗證資料庫連線 |

### 後端優先級 2: 認證授權 (12h)
| Task | 名稱 | 工時 |
|------|------|------|
| 2.1 | LDAP 認證 | 3h |
| 2.2 | JWT 實現 | 3h |
| 2.3 | AuthController | 2h |
| 2.4 | RBAC 授權 | 2h |
| 2.5 | 測試認證流程 | 2h |

### 前端優先級 0: 基礎設施 (8.5h)
| Task | 名稱 | 工時 | 內容 |
|------|------|------|------|
| 0.1 | React 專案建立 | 1h | Vite + React 初始化 |
| 0.2 | 套件安裝 | 1h | 依賴管理 |
| 0.3 | AuthContext 建立 | 3h | JWT Token 管理 |
| 0.4 | API 客戶端 | 3.5h | Axios 設置 + 攔截器 |

### 前端優先級 A: 核心功能 (15.5h)
| Task | 名稱 | 工時 | 內容 |
|------|------|------|------|
| A.1 | 登入表單 | 2.5h | Formik + 驗證 |
| A.2 | 預約表單 | 3h | 複雜表單設計 |
| A.3 | 預約卡片列表 | 2.5h | 響應式列表 |
| A.4 | 日程視圖 | 3h | React Big Calendar |
| A.5 | 確認對話框 | 2h | 使用者確認 UI |
| A.6 | 休假管理 | 2.5h | 休假排程表單 |

---

## 🔌 API 端點速查表

### 認證相關
```
POST   /api/auth/login              登入
POST   /api/auth/verify             驗證 Token
```

### 預約相關
```
POST   /api/appointments            建立預約
GET    /api/appointments            取得列表
GET    /api/appointments/{id}       取得詳情
PATCH  /api/appointments/{id}/confirm     確認預約
PATCH  /api/appointments/{id}/reject      拒絕預約
```

### 異議相關
```
POST   /api/appointments/{id}/objections  提交異議
GET    /api/objections              取得異議列表
```

### 休假相關
```
POST   /api/leave-schedules         新增休假
GET    /api/leave-schedules         取得休假
```

📖 **完整規約**: [`contracts/openapi.yaml`](./contracts/openapi.yaml)

---

## 💾 資料模型速查

### 核心實體
```
User
├─ id: GUID
├─ adAccount: string (WiAD 帳號)
├─ name: string
├─ email: string
├─ role: enum (applicant, reviewer)
└─ isActive: bool

Appointment
├─ id: GUID
├─ objectName: string (合約名稱)
├─ timeStart: DateTime
├─ timeEnd: DateTime
├─ status: enum (pending, confirmed, rejected)
├─ applicantId: GUID (FK User)
├─ reviewerId: GUID (FK User, nullable)
└─ createdAt: DateTime

LeaveSchedule
├─ id: GUID
├─ userId: GUID (FK User)
├─ startDate: Date
├─ endDate: Date
└─ reason: string

AppointmentHistory (審計)
├─ id: GUID
├─ appointmentId: GUID (FK)
├─ action: enum (created, confirmed, rejected)
├─ performedBy: GUID (FK User)
└─ timestamp: DateTime

NotificationLog (郵件紀錄)
├─ id: GUID
├─ recipient: string
├─ subject: string
├─ status: enum (pending, sent, failed)
└─ sentAt: DateTime?
```

📖 **完整定義**: [`data-model.md`](./data-model.md)

---

## 🗂️ 檔案目錄結構

### 後端 (ASP.NET Core)
```
src/
├─ Controllers/
│  ├─ AuthController.cs
│  ├─ AppointmentsController.cs
│  └─ LeaveSchedulesController.cs
├─ Models/
│  ├─ User.cs
│  ├─ Appointment.cs
│  └─ LeaveSchedule.cs
├─ Services/
│  ├─ AuthService.cs
│  ├─ AppointmentService.cs
│  └─ EmailService.cs
├─ Data/
│  ├─ ApplicationDbContext.cs
│  └─ Migrations/
├─ Filters/
│  └─ JwtAuthenticationFilter.cs
└─ appsettings.json
```

### 前端 (React + TypeScript)
```
src/
├─ components/
│  ├─ Layout/
│  │  ├─ AppShell.tsx
│  │  └─ Navigation.tsx
│  ├─ Auth/
│  │  ├─ LoginForm.tsx
│  │  └─ LoginPage.tsx
│  ├─ Appointments/
│  │  ├─ AppointmentForm.tsx
│  │  ├─ AppointmentCard.tsx
│  │  └─ AppointmentList.tsx
│  └─ Calendar/
│     └─ CalendarView.tsx
├─ pages/
│  ├─ LoginPage.tsx
│  ├─ DashboardPage.tsx
│  └─ CalendarPage.tsx
├─ contexts/
│  └─ AuthContext.tsx
├─ hooks/
│  ├─ useAppointments.ts
│  └─ useAuth.ts
├─ api/
│  ├─ client.ts
│  ├─ appointments.ts
│  └─ auth.ts
├─ types/
│  └─ index.ts
├─ App.tsx
└─ main.tsx
```

---

## 🎯 常見任務快速指令

### 後端 (ASP.NET Core)

**建立新控制器**
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet aspnet-codegenerator controller -name MyController -async -api
```

**執行資料庫遷移**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**執行測試**
```bash
dotnet test
```

**執行應用**
```bash
dotnet run
```

### 前端 (React)

**建立新元件**
```bash
npm create vite@latest my-project -- --template react-ts
```

**安裝套件**
```bash
npm install react-bootstrap bootstrap axios formik yup react-big-calendar
```

**執行開發伺服器**
```bash
npm run dev    # Vite 開發伺服器 (http://localhost:5173)
```

**執行測試**
```bash
npm run test
```

**構建生產版**
```bash
npm run build
```

---

## 🔍 除錯技巧

### 後端常見問題

| 問題 | 解決方案 |
|------|---------|
| EF Core 遷移失敗 | 檢查連接字符串 + 資料庫權限 |
| LDAP 認證失敗 | 驗證 LDAP 連接參數 + WiAD 帳號 |
| JWT Token 無效 | 檢查簽署密鑰 + 過期時間 |
| 郵件未發送 | 檢查 SMTP 設定 + 郵箱密碼 |

### 前端常見問題

| 問題 | 解決方案 |
|------|---------|
| API 連接失敗 | 檢查 VITE_API_URL 環境變數 |
| CORS 錯誤 | 檢查後端 CORS 配置 |
| TypeScript 錯誤 | 執行 `npm run build` 檢查編譯 |
| 熱重載不工作 | 重啟開發伺服器 |

---

## 📞 快速聯絡

### 文件問題
- 功能定義不清: → [`spec.md`](./spec.md)
- 技術決策疑問: → [`research.md`](./research.md) / [`frontend-research.md`](./frontend-research.md)
- API 規約查詢: → [`contracts/openapi.yaml`](./contracts/openapi.yaml)

### 開發環境問題
- 後端環境: → [`quickstart.md`](./quickstart.md)
- 前端環境: → [`frontend-quickstart.md`](./frontend-quickstart.md)

### 工作任務問題
- 任務清單: → [`tasks.md`](./tasks.md)
- 工期估算: → [`tasks.md`](./tasks.md) 中的工時欄位
- 優先級安排: → [`tasks.md`](./tasks.md) 中的優先級分類

---

## ✅ 定期檢查清單

### 每日開發前
- ✅ 同步最新 Git 分支
- ✅ 檢查當日任務優先級
- ✅ 確認 API 是否有更新

### 每周結束
- ✅ 單元測試覆蓋率 ≥80%
- ✅ 代碼審查通過
- ✅ 更新任務進度狀態

### 進度里程碑
- ✅ W1 結束: 基礎設施完成 (後端 Task 1, 前端 Task 0)
- ✅ W3 結束: 認證系統完成 (後端 Task 2)
- ✅ W5 結束: 核心功能完成 (後端 Task 3-4, 前端 Task A)
- ✅ W7 結束: 整體完成 (測試 + 部署準備)

---

**此快速參考指南旨在加速查詢。如需詳細說明，請參考完整文件。** 📖

**版本**: 2.0 | **最後更新**: 2025-11-18 | **狀態**: ✅ 有效
