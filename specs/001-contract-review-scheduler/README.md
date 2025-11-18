# 契約審查預約系統 (001-contract-review-scheduler) - 規劃文件索引

**系統名稱**: 契約審查預約系統 (Contract Review Appointment System)  
**分支**: `001-contract-review-scheduler`  
**技術棧**: ASP.NET Core 8.0 + SQL Server 2019+ + EF Core  
**狀態**: 規劃完成，準備進入 Phase 2 實施  
**最後更新**: 2025-11-18  

---

## 📋 規劃文件導覽

### Phase 0: 規格與需求分析

#### [`spec.md`](./spec.md)
**用途**: 功能規格書  
**包含內容**:
- 功能概述和背景
- 4 個核心使用案例
- 5 個功能需求領域
- 成功準則
- 相關實體和流程

**讀者**: 產品經理、設計師、開發者  
**狀態**: ✅ 完成

---

#### [`checklists/requirements.md`](./checklists/requirements.md)
**用途**: 規格驗證檢查清單  
**包含內容**:
- 功能完整性檢查
- 需求可追蹤性檢查
- 相關方簽核

**讀者**: 品質保證、專案經理  
**狀態**: ✅ 完成 (所有項目已驗證)

---

### Phase 1: 設計與技術決策

#### [`research.md`](./research.md)
**用途**: 技術決策研究文件  
**包含內容**:
- 11 個 ASP.NET Core 核心技術決策
  1. WiAD/LDAP 認證 (System.DirectoryServices)
  2. 郵件系統 (System.Net.Mail + IHostedService)
  3. 資料庫 (SQL Server)
  4. ORM (Entity Framework Core Code First)
  5. POCO 物件映射 (無 AutoMapper)
  6. 記憶體快取 (IMemoryCache，無 Redis)
  7. 控制器架構 (傳統 Controllers，無 Minimal APIs)
  8. 認證 (LDAP + JWT)
  9. 日誌 (Serilog)
  10. 效能最佳化
  11. 安全考量

**讀者**: 技術主管、架構師、開發者  
**狀態**: ✅ 完成

---

#### [`data-model.md`](./data-model.md)
**用途**: 資料庫設計與 EF Core 配置  
**包含內容**:
- EF Core Code First 工作流程
- 5 個 POCO 實體類別定義 (含 C# 代碼)
  - User
  - Appointment
  - LeaveSchedule
  - AppointmentHistory
  - NotificationLog
- ApplicationDbContext 完整配置 (含 Fluent API)
- 複合索引和外鍵設定
- EF Core 遷移指令

**讀者**: 資料庫管理員、後端開發者  
**狀態**: ✅ 完成

---

#### [`contracts/openapi.yaml`](./contracts/openapi.yaml)
**用途**: REST API 規約  
**包含內容**:
- OpenAPI 3.0 規範
- 11 個 API 端點
- 4 個功能分類 (Auth, Calendar, Appointments, LeaveSchedules)
- 5 個核心資料結構
- 請求/回應範例
- 認證方案 (JWT Bearer)

**讀者**: 前端開發者、API 使用者、QA  
**狀態**: ✅ 完成

---

#### [`quickstart.md`](./quickstart.md)
**用途**: 開發者快速開始指南  
**包含內容**:
- 環境先決條件 (.NET 8 SDK, Visual Studio 2022, SQL Server)
- 三種設置方式 (VS2022, CLI, Docker)
- 專案結構說明
- 常用命令參考
- 常見問題解答
- 部署準備檢查清單

**讀者**: 新進開發者、DevOps 工程師  
**狀態**: ✅ 完成

---

### Phase 2: 實施計劃與任務分解

#### [`plan.md`](./plan.md)
**用途**: 整體實施計劃與進度追蹤  
**包含內容**:
- 技術背景和選擇理由
- 憲章合規性檢查 (6 項原則)
- Phase 0 研究總結 (6 項決策)
- Phase 1 設計成果
- Phase 2 任務分解概述
- 門檻檢查
- 產出檔案清單

**讀者**: 專案經理、技術主管  
**狀態**: ✅ 完成 (Phase 0-1 完成，Phase 2 準備中)

---

#### [`tasks.md`](./tasks.md) ⭐ **開始實施的起點**
**用途**: 詳細任務分解與實施指南  
**包含內容**:
- **33 個任務**分為 7 個優先級:
  - 優先級 1️⃣ (5 任務): 基礎設施 - 7.5 小時
  - 優先級 2️⃣ (5 任務): 認證授權 - 12 小時
  - 優先級 3️⃣ (5 任務): 預約核心 - 12 小時
  - 優先級 4️⃣ (5 任務): 審查人員 - 9 小時
  - 優先級 5️⃣ (5 任務): 郵件系統 - 9 小時
  - 優先級 6️⃣ (6 任務): 測試 QA - 19 小時
  - 優先級 7️⃣ (3 任務): 文件部署 - 7 小時

**每個任務包含**:
- 詳細工作項清單
- 驗收準則
- 預估工時
- 相依任務關係
- 程式碼範例
- 最佳實踐

**預計工期**: 75 小時 (1 名開發者 6 周)

**讀者**: 開發者、QA 工程師、專案經理  
**狀態**: ✅ 完成 (Phase 2 詳細計劃)

---

## 📊 成品矩陣

| 檔案 | 類型 | 完成度 | 用途 | 讀者 |
|------|------|--------|------|------|
| `spec.md` | 規格 | ✅ 100% | 功能定義 | PM, Design |
| `plan.md` | 計劃 | ✅ 100% | 實施路線 | PM, Tech Lead |
| `research.md` | 技術決策 | ✅ 100% | 架構決策 | Arch, Dev |
| `data-model.md` | 設計 | ✅ 100% | DB 架構 | DBA, Dev |
| `contracts/openapi.yaml` | 規約 | ✅ 100% | API 合約 | Dev, QA |
| `quickstart.md` | 指南 | ✅ 100% | 快速開始 | Onboarding |
| `tasks.md` | 任務 | ✅ 100% | 實施細節 | Dev, QA, PM |

---

## 🚀 建議閱讀順序

### 角色別導覽

#### 👨‍💼 **產品經理 / 專案經理**
1. 閱讀 [`spec.md`](./spec.md) - 了解功能
2. 閱讀 [`plan.md`](./plan.md) - 了解進度
3. 參考 [`tasks.md`](./tasks.md) 追蹤工期

**預計時間**: 1-2 小時

---

#### 🏗️ **架構師 / 技術主管**
1. 閱讀 [`research.md`](./research.md) - 了解技術決策
2. 閱讀 [`data-model.md`](./data-model.md) - 檢視 DB 設計
3. 參考 [`contracts/openapi.yaml`](./contracts/openapi.yaml) - API 架構

**預計時間**: 2-3 小時

---

#### 👨‍💻 **後端開發者**
1. 閱讀 [`quickstart.md`](./quickstart.md) - 環境設置
2. 閱讀 [`data-model.md`](./data-model.md) - 資料模型和 DbContext
3. 參考 [`contracts/openapi.yaml`](./contracts/openapi.yaml) - API 端點
4. 使用 [`tasks.md`](./tasks.md) - 按優先級實施

**預計時間**: 第一天整天

---

#### 🔍 **測試工程師**
1. 閱讀 [`spec.md`](./spec.md) - 功能需求
2. 閱讀 [`checklists/requirements.md`](./checklists/requirements.md) - 測試案例清單
3. 參考 [`contracts/openapi.yaml`](./contracts/openapi.yaml) - API 測試規約
4. 使用 [`tasks.md`](./tasks.md) 中 Task 6 - 測試設計

**預計時間**: 2-3 小時

---

#### 🚀 **DevOps / 系統管理員**
1. 閱讀 [`quickstart.md`](./quickstart.md) - 部署需求
2. 參考 [`tasks.md`](./tasks.md) 中 Task 7.3 - CI/CD 設置
3. 檢視 [`data-model.md`](./data-model.md) - 資料庫先決條件

**預計時間**: 1 小時

---

## 🎯 進度查看清單

### 規劃階段 (已完成)
- ✅ 功能規格完成
- ✅ 技術決策完成
- ✅ 資料模型完成
- ✅ API 規約完成
- ✅ 任務分解完成

### 實施階段 (準備中)
- ⏳ Task 1.1-1.5: 基礎設施 (預計 1 周)
- ⏳ Task 2.1-2.5: 認證授權 (預計 2 周)
- ⏳ Task 3.1-3.5: 預約核心 (預計 2 周)
- ⏳ Task 4.1-4.5: 審查人員 (預計 1.5 周)
- ⏳ Task 5.1-5.5: 郵件系統 (預計 1.5 周)
- ⏳ Task 6.1-6.6: 測試 QA (預計 3 周)
- ⏳ Task 7.1-7.3: 文件部署 (預計 1 周)

### 驗收階段 (待開始)
- ⏹️ 功能驗收
- ⏹️ 效能驗收
- ⏹️ 安全性驗收
- ⏹️ 文件審查

---

## 📞 問題排查指南

### 常見問題

**Q: 我應該從哪裡開始?**  
A: 如果你是開發者，從 [`quickstart.md`](./quickstart.md) 和 [`tasks.md`](./tasks.md) 開始。

**Q: 如何了解系統功能?**  
A: 閱讀 [`spec.md`](./spec.md)，它包含所有使用案例和功能。

**Q: 資料庫如何設計?**  
A: 參考 [`data-model.md`](./data-model.md)，其中包含完整的 POCO 實體和 DbContext 配置。

**Q: API 端點有哪些?**  
A: 查看 [`contracts/openapi.yaml`](./contracts/openapi.yaml)，可用 Swagger 在本地運行查看。

**Q: 工期預估是多少?**  
A: 根據 [`tasks.md`](./tasks.md)，全職開發者需要 6 周 (~75 小時)。

**Q: 哪些決策是必須遵循的?**  
A: [`research.md`](./research.md) 中的 11 個決策是必須的。如需更改，需獲得技術主管批准。

---

## 📝 文件版本歷史

| 日期 | 版本 | 變更 |
|------|------|------|
| 2025-11-18 | 1.0 | 初始版本發布，所有規劃文件完成 |
| | | - 功能規格 v1.0 (5 項澄清決策整合) |
| | | - 技術決策 v1.0 (11 項 ASP.NET Core 決策) |
| | | - 資料模型 v1.0 (5 POCO 實體 + DbContext) |
| | | - API 規約 v1.0 (OpenAPI 3.0) |
| | | - 任務分解 v1.0 (33 項任務) |

---

## 📚 相關文件

### 專案根目錄
- [`.specify/memory/constitution.md`](../../.specify/memory/constitution.md) - 專案憲章 v2.0.0
  - 6 項核心原則 (代碼品質、測試、UX、效能、安全、文件語言)
  - 所有成品必須遵循

### 外部參考
- [ASP.NET Core 官方文檔](https://learn.microsoft.com/dotnet/core/aspnet)
- [Entity Framework Core 文檔](https://learn.microsoft.com/ef/core/)
- [OpenAPI 規範](https://spec.openapis.org/oas/latest.html)

---

**本文件是規劃的入口點。建議加入書籤以便快速存取。**

**準備開始實施? 轉到 [`tasks.md`](./tasks.md) 並按優先級開始。** 🚀
