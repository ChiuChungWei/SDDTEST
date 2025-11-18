# 契約審查預約系統 - 任務清單 (Phase 2: 實施計劃)

**特性**: 001-contract-review-scheduler  
**日期**: 2025-11-18  
**狀態**: Phase 2 - 任務分解  
**技術棧**: ASP.NET Core 8.0, SQL Server 2019+, Entity Framework Core, React 19.2.0, TypeScript 5.6  
**讀取來源**: `/specs/001-contract-review-scheduler/` (plan.md, spec.md, data-model.md, contracts/openapi.yaml, research.md)

---

## 總覽

### 使用者故事映射

| 故事 | 優先級 | 標題 | 關鍵功能 |
|------|--------|------|---------|
| US1 | P1 | 申請人預約契約審查 | 登入、月曆選擇、預約表單、時段驗證、郵件通知 |
| US2 | P2 | 審查人員管理預約 | 檢視預約、接受/拒絕、黃色標記 |
| US3 | P2 | 審查人員設定休假 | 月曆、休假設定、時段驗證 |
| US4 | P3 | 轉送預約給代理人 | 代理人選擇、轉送請求、接受/拒絕、狀態更新 |

### 技術棧概要

- **後端**: ASP.NET Core 8.0 Web API、SQL Server、Entity Framework Core (Code First)
- **認證**: System.DirectoryServices (LDAP) + JWT Token
- **郵件**: System.Net.Mail + IHostedService (後台佇列)
- **快取**: IMemoryCache (AD 快取、預約清單快取)
- **前端**: React 19.2.0、TypeScript、Bootstrap 5.3、React Router、Context API、Axios
- **API 規約**: OpenAPI 3.0 (11 個端點)

### 實體映射

| 實體 | 關聯故事 | 功能 |
|------|---------|------|
| User | 所有故事 | 系統使用者（申請人/審查人員） |
| Appointment | US1, US2, US4 | 預約主體 |
| LeaveSchedule | US3 | 審查人員休假排程 |
| AppointmentHistory | US2, US4 | 預約操作歷史 |
| NotificationLog | 所有故事 | 郵件通知記錄 |

### API 端點映射（來自 contracts/openapi.yaml）

| 端點 | 方法 | 關聯故事 | 功能 |
|------|------|---------|------|
| `/api/auth/login` | POST | 所有 | 使用者登入 |
| `/api/auth/logout` | POST | 所有 | 使用者登出 |
| `/api/calendar/{reviewerId}/{date}` | GET | US1, US3 | 取得月曆資訊 |
| `/api/appointments` | POST | US1 | 建立預約 |
| `/api/appointments/{id}` | GET | US1, US2 | 取得預約詳情 |
| `/api/appointments/{id}/accept` | PUT | US2 | 審查人員接受預約 |
| `/api/appointments/{id}/reject` | PUT | US2 | 審查人員拒絕預約 |
| `/api/appointments/{id}/delegate` | PUT | US4 | 轉送預約 |
| `/api/leave-schedules` | POST | US3 | 建立休假排程 |
| `/api/leave-schedules/{id}` | DELETE | US3 | 刪除休假排程 |
| `/api/users` | GET | 管理 | 取得使用者清單 |

---

## Phase 1: 設置 (共享基礎設施)

**目的**: 專案初始化、資料庫設置、基礎中間件

### Phase 1 獨立測試準則
- ✅ 專案能成功編譯和執行
- ✅ 資料庫已建立，所有表格存在
- ✅ API 在 Swagger 中可見
- ✅ 結構化日誌已配置並輸出

### 實施任務

- [x] T001 創建 ASP.NET Core 8.0 Web API 專案結構，位置 `backend/` 根目錄
- [x] T002 配置 NuGet 依賴套件 (EF Core, Serilog, System.DirectoryServices, JWT, 等) 在 `backend/ContractReviewScheduler.csproj`
- [x] T003 [P] 設置 SQL Server 連線字串在 `backend/appsettings.Development.json`
- [x] T004 [P] 配置 Entity Framework Core DbContext 在 `backend/Data/ApplicationDbContext.cs`
- [x] T005 創建初始 EF Core 遷移並應用到資料庫，執行指令在 `backend/` 目錄
- [x] T006 [P] 配置 Serilog 結構化日誌在 `backend/Program.cs`
- [x] T007 [P] 創建全域例外處理中間件在 `backend/Middleware/ExceptionHandlingMiddleware.cs`

**Phase 1 檢查點**: 後端框架已初始化，資料庫已就緒，可開始業務邏輯實施

---

## Phase 2: 基礎設施 (阻斷性先決條件)

**目的**: 認證、授權、快取系統 - 所有使用者故事的前置條件

**⚠️ 關鍵**: 此階段完成前，任何使用者故事都無法開始實施

### Phase 2 獨立測試準則
- ✅ LDAP 驗證成功，使用者可以登入
- ✅ JWT Token 正確簽發且可驗證
- ✅ 角色型存取控制 (RBAC) 可正常運作
- ✅ 快取策略可減少 AD 查詢

### 實施任務

- [x] T008 實施 LDAP 認證服務在 `backend/Services/LdapService.cs` (System.DirectoryServices 整合)
- [x] T009 [P] 實施 IMemoryCache 快取層在 `backend/Services/CacheService.cs` (AD 使用者快取 1 小時 TTL)
- [x] T010 [P] 實施 JWT Token 簽發與驗證在 `backend/Services/JwtService.cs`
- [x] T011 [P] 實施 RBAC 授權中間件在 `backend/Middleware/RoleAuthorizationMiddleware.cs`
- [x] T012 實施使用者同步服務在 `backend/Services/UserSyncService.cs`

**Phase 2 檢查點**: 認證系統已完成，使用者故事實施可並行進行

---

## Phase 3: 使用者故事 1 - 申請人預約契約審查 (優先級: P1) 🎯 MVP

**目標**: 申請人能透過系統登入、選擇日期、填寫預約表單、完成預約，系統自動發送郵件通知審查人員

**獨立測試**: 申請人可在 3 分鐘內完成端到端預約流程，審查人員收到通知郵件

### US1 合約測試 (先寫後實施)

- [ ] T013 [P] [US1] 編寫預約建立端點的合約測試 `backend/Tests/Contract/CreateAppointmentContractTest.cs`
- [ ] T014 [P] [US1] 編寫時段衝突偵測合約測試 `backend/Tests/Contract/ConflictDetectionContractTest.cs`
- [ ] T015 [P] [US1] 編寫月曆 API 合約測試 `backend/Tests/Contract/CalendarEndpointContractTest.cs`

### US1 實施任務

- [ ] T016 [P] [US1] 創建 User 模型在 `backend/Models/Domain/User.cs` (ad_account, name, email, role, is_active)
- [ ] T017 [P] [US1] 創建 Appointment 模型在 `backend/Models/Domain/Appointment.cs` (含狀態、代理人字段)
- [x] T018 [US1] 實施 AppointmentService 商業邏輯在 `backend/Services/AppointmentService.cs` (驗證時段、檢查衝突、檢查休假)
- [x] T019 [P] [US1] 實施時段衝突偵測演算法在 `backend/Services/ConflictDetectionService.cs` (使用 SQL Server DATEDIFF)
- [x] T020 [P] [US1] 實施月曆資訊 API 端點在 `backend/Controllers/CalendarController.cs` (GET /api/calendar/{reviewerId}/{date})
- [x] T021 [US1] 實施預約建立 API 端點在 `backend/Controllers/AppointmentsController.cs` (POST /api/appointments)
- [x] T022 [US1] 實施預約查詢 API 端點在 `backend/Controllers/AppointmentsController.cs` (GET /api/appointments/{id})
- [ ] T023 [P] [US1] 實施郵件通知後台服務在 `backend/HostedServices/EmailQueueService.cs` (IHostedService)
- [ ] T024 [P] [US1] 創建郵件範本在 `backend/Templates/NewAppointmentNotification.html`
- [ ] T025 [US1] 整合郵件系統於預約建立流程

### US1 前端任務

- [ ] T026 [P] [US1] 創建登入頁面元件 `frontend/src/pages/LoginPage.tsx` (表單驗證、錯誤處理)
- [ ] T027 [P] [US1] 創建月曆檢視元件 `frontend/src/components/CalendarView.tsx` (使用 react-big-calendar)
- [ ] T028 [P] [US1] 創建預約表單元件 `frontend/src/components/AppointmentForm.tsx` (Formik + Yup 驗證)
- [ ] T029 [US1] 實施預約建立工作流程整合 (登入 → 月曆選擇 → 表單填寫 → 提交)
- [ ] T030 [US1] 實施成功通知和錯誤處理 UI

**Phase 3 檢查點**: US1 完整可運作，申請人可從 A 到 Z 完成預約

---

## Phase 4: 使用者故事 2 - 審查人員管理預約 (優先級: P2)

**目標**: 審查人員登入後能檢視預約清單（黃色標記待確認項目）、接受或拒絕預約，系統發送確認/拒絕郵件

**獨立測試**: 審查人員可在 1 分鐘內接受或拒絕預約，申請人收到對應通知

### US2 合約測試

- [ ] T031 [P] [US2] 編寫接受預約端點合約測試 `backend/Tests/Contract/AcceptAppointmentContractTest.cs`
- [ ] T032 [P] [US2] 編寫拒絕預約端點合約測試 `backend/Tests/Contract/RejectAppointmentContractTest.cs`

### US2 實施任務

- [ ] T033 [US2] 創建 AppointmentHistory 模型在 `backend/Models/Domain/AppointmentHistory.cs` (追蹤操作歷史)
- [ ] T034 [P] [US2] 實施預約接受端點在 `backend/Controllers/AppointmentsController.cs` (PUT /api/appointments/{id}/accept)
- [ ] T035 [P] [US2] 實施預約拒絕端點在 `backend/Controllers/AppointmentsController.cs` (PUT /api/appointments/{id}/reject)
- [ ] T036 [US2] 實施預約狀態轉換邏輯在 `backend/Services/AppointmentService.cs`
- [ ] T037 [P] [US2] 實施審查人員預約清單查詢在 `backend/Controllers/AppointmentsController.cs` (GET /api/appointments?reviewerId=...)
- [ ] T038 [P] [US2] 創建確認/拒絕郵件範本在 `backend/Templates/AppointmentConfirmed.html` 和 `backend/Templates/AppointmentRejected.html`

### US2 前端任務

- [ ] T039 [P] [US2] 創建預約清單元件 `frontend/src/components/AppointmentList.tsx` (顯示待確認項目為黃色)
- [ ] T040 [P] [US2] 創建預約詳情面板 `frontend/src/components/AppointmentDetailPanel.tsx`
- [ ] T041 [US2] 實施接受/拒絕預約按鈕與確認對話框
- [ ] T042 [US2] 實施状態更新後的即時 UI 刷新

**Phase 4 檢查點**: US2 完整可運作，審查人員工作流已就緒

---

## Phase 5: 使用者故事 3 - 審查人員設定休假 (優先級: P2)

**目標**: 審查人員在月曆上選擇日期設定休假時段，系統防止該時段的預約建立

**獨立測試**: 系統正確防止 100% 的時段重複預約（包括休假檢查）

### US3 合約測試

- [ ] T043 [P] [US3] 編寫休假設定端點合約測試 `backend/Tests/Contract/CreateLeaveScheduleContractTest.cs`

### US3 實施任務

- [ ] T044 [P] [US3] 創建 LeaveSchedule 模型在 `backend/Models/Domain/LeaveSchedule.cs` (reviewer_id, date, time_start, time_end)
- [ ] T045 [P] [US3] 實施休假設定 API 端點在 `backend/Controllers/LeaveSchedulesController.cs` (POST /api/leave-schedules)
- [ ] T046 [P] [US3] 實施休假刪除 API 端點在 `backend/Controllers/LeaveSchedulesController.cs` (DELETE /api/leave-schedules/{id})
- [ ] T047 [US3] 在衝突偵測中整合休假檢查邏輯 (修改 `backend/Services/ConflictDetectionService.cs`)
- [ ] T048 [P] [US3] 實施休假清單查詢在 `backend/Controllers/LeaveSchedulesController.cs` (GET /api/leave-schedules?reviewerId=...)

### US3 前端任務

- [ ] T049 [P] [US3] 創建休假設定表單元件 `frontend/src/components/LeaveScheduleForm.tsx`
- [ ] T050 [P] [US3] 在月曆中視覺化顯示休假時段（例如灰色背景）
- [ ] T051 [US3] 實施休假刪除確認對話框

**Phase 5 檢查點**: US3 完整可運作，時段衝突防護已完全實施

---

## Phase 6: 使用者故事 4 - 轉送預約給代理人 (優先級: P3)

**目標**: 審查人員將已確認的預約轉送給其他審查人員（代理人），代理人接受或拒絕轉送，相關人員收到通知

**獨立測試**: 代理人流程可獨立測試，包括轉送請求、接受、拒絕、狀態更新、郵件通知

### US4 合約測試

- [ ] T052 [P] [US4] 編寫預約轉送端點合約測試 `backend/Tests/Contract/DelegateAppointmentContractTest.cs`
- [ ] T053 [P] [US4] 編寫轉送接受/拒絕端點合約測試 `backend/Tests/Contract/DelegateDealingContractTest.cs`

### US4 實施任務

- [ ] T054 [P] [US4] 在 Appointment 模型中添加代理人字段 (delegate_reviewer_id, delegate_status) - 修改 `backend/Models/Domain/Appointment.cs`
- [ ] T055 [P] [US4] 實施預約轉送 API 端點在 `backend/Controllers/AppointmentsController.cs` (PUT /api/appointments/{id}/delegate)
- [ ] T056 [P] [US4] 實施轉送接受 API 端點在 `backend/Controllers/AppointmentsController.cs` (PUT /api/appointments/{id}/delegate-accept)
- [ ] T057 [P] [US4] 實施轉送拒絕 API 端點在 `backend/Controllers/AppointmentsController.cs` (PUT /api/appointments/{id}/delegate-reject)
- [ ] T058 [US4] 在 AppointmentService 中實施代理人處理邏輯 (修改 `backend/Services/AppointmentService.cs`)
- [ ] T059 [P] [US4] 創建轉送郵件範本在 `backend/Templates/AppointmentDelegated.html`

### US4 前端任務

- [ ] T060 [P] [US4] 創建代理人選擇下拉選單 `frontend/src/components/DelegateReviewerSelect.tsx`
- [ ] T061 [P] [US4] 創建轉送確認對話框 `frontend/src/components/DelegatConfirmDialog.tsx`
- [ ] T062 [US4] 在預約詳情中顯示代理人狀態
- [ ] T063 [US4] 為待轉送項目創建獨立清單檢視

**Phase 6 檢查點**: US4 完整可運作，轉送工作流已完成

---

## Phase 7: 測試與品質保證

**目的**: 單元測試、整合測試、端到端測試、性能測試

### 後端測試任務

- [ ] T064 [P] 編寫 AppointmentService 單元測試 `backend/Tests/Unit/AppointmentServiceTests.cs` (覆蓋 80% 以上)
- [ ] T065 [P] 編寫 ConflictDetectionService 單元測試 `backend/Tests/Unit/ConflictDetectionServiceTests.cs`
- [ ] T066 [P] 編寫 LdapService 單元測試 `backend/Tests/Unit/LdapServiceTests.cs`
- [ ] T067 編寫 AppointmentsController 整合測試 `backend/Tests/Integration/AppointmentsControllerIntegrationTests.cs`
- [ ] T068 編寫端到端測試場景（使用 Postman/Thunder Client）在 `backend/Tests/E2E/scenarios.json`
- [ ] T069 [P] 性能測試與最佳化 (測試 < 200ms 回應時間)

### 前端測試任務

- [ ] T070 [P] 編寫 React 元件單元測試 `frontend/src/components/__tests__/AppointmentForm.test.tsx` (使用 Vitest + React Testing Library)
- [ ] T071 [P] 編寫整合測試 `frontend/src/__tests__/integration/bookingFlow.test.tsx`
- [ ] T072 編寫 E2E 測試 (使用 Cypress) `frontend/e2e/booking.spec.ts`

**Phase 7 檢查點**: 所有測試已通過，程式碼涵蓋率 ≥ 80%

---

## Phase 8: 文件與部署

**目的**: API 文件、部署指南、環境設置清單

### 後端任務

- [ ] T073 配置 Swagger/OpenAPI 文件在 `backend/Program.cs` (自動從程式碼產生)
- [ ] T074 編寫部署準備清單 `backend/DEPLOYMENT_CHECKLIST.md`
- [ ] T075 準備 Docker 配置檔 `backend/Dockerfile` 和 `backend/docker-compose.yml`

### 前端任務

- [ ] T076 [P] 配置 Vite 構建優化在 `frontend/vite.config.ts`
- [ ] T077 [P] 編寫部署指南 `frontend/DEPLOYMENT_GUIDE.md`
- [ ] T078 編寫環境變數配置範本 `frontend/.env.example`

**Phase 8 檢查點**: 系統已準備好部署至測試環境

---

## 任務依賴圖

```
Phase 1 (設置)
    ↓
Phase 2 (認證基礎設施)
    ↓
┌─────────────────────────────────────────────┐
│ Phase 3 (US1)  Phase 4 (US2)  Phase 5 (US3) │ [可並行]
└─────────────────────────────────────────────┘
    ↓
Phase 6 (US4) [依賴於 US1, US2, US3]
    ↓
Phase 7 (測試)
    ↓
Phase 8 (文件與部署)
```

---

## 並行執行機會

### 推薦團隊編制：2 名開發者

**團隊 A (後端開發者)**:
- Phase 1-2 序列執行 (基礎設施)
- Phase 3-5 中與團隊 B 並行實施 (後端 API)
- Phase 6-8 序列執行

**團隊 B (前端開發者)**:
- Phase 3-5 中與團隊 A 並行實施 (前端 UI)
- Phase 7-8 與團隊 A 並行測試和文件

### 預計工期

| 階段 | 後端工時 | 前端工時 | 預期週次 |
|------|---------|---------|---------|
| Phase 1-2 | 19.5h | - | 第 1-2 周 |
| Phase 3-5 | 36h | 36h | 第 2-4 周 (並行) |
| Phase 6 | 9h | 8h | 第 4-5 周 (並行) |
| Phase 7 | 19h | 12h | 第 5-6 周 (並行) |
| Phase 8 | 7h | 3h | 第 6-7 周 |
| **總計** | **~90h** | **~59h** | **7 周 (2 人)** |

---

## 驗收準則 (每個故事)

### US1 驗收準則
- ✅ 申請人可在 3 分鐘內完成預約
- ✅ 審查人員收到郵件通知
- ✅ 時段衝突 100% 被防止
- ✅ 系統回應時間 < 2 秒

### US2 驗收準則
- ✅ 審查人員可在 1 分鐘內接受/拒絕預約
- ✅ 申請人收到確認/拒絕通知
- ✅ 待確認項目正確顯示為黃色

### US3 驗收準則
- ✅ 系統正確防止休假時段的預約
- ✅ 審查人員可自由設置/刪除休假

### US4 驗收準則
- ✅ 轉送流程完整可運作
- ✅ 相關人員收到轉送通知
- ✅ 代理人可接受/拒絕轉送

---

## 任務格式驗證

✅ **所有任務遵循嚴格格式**:
- `- [ ]` 複選框
- 任務 ID (T001-T078)
- `[P]` 標籤（可並行任務）
- `[US#]` 標籤（故事關聯，Phase 3+ 必須）
- 清晰的文件路徑

✅ **獨立測試標準**:
- 每個故事可獨立實施、測試、部署
- Phase 1-2 是所有故事的先決條件
- Phase 3-5 可並行執行
- Phase 6 依賴前面所有故事

✅ **任務完整性**:
- 78 個任務總計
- 後端: 49 個任務
- 前端: 29 個任務
- 涵蓋所有 4 個使用者故事

---

**文件版本**: 1.0  
**最後更新**: 2025-11-18  
**狀態**: 準備開始 Phase 1 實施



