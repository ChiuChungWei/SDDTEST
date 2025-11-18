# 契約審查預約系統 - 實作計畫

**功能分支**: `001-contract-review-scheduler`  
**建立日期**: 2025-11-18  
**更新日期**: 2025-11-18 (ASP.NET Core 8.0 版本)  
**狀態**: 規劃中

## 技術背景

### 選定技術棧
- **框架**: ASP.NET Core 8.0 Web API
- **程式語言**: C#
- **資料庫**: SQL Server 2019+
- **ORM**: Entity Framework Core (Code First)
- **認證**: LDAP (System.DirectoryServices) + JWT
- **郵件**: System.Net.Mail + 後台服務 (IHostedService)
- **快取**: IMemoryCache (System.Runtime.Caching)
- **日誌**: Serilog
- **API 設計**: RESTful (控制器，不使用 Minimal APIs)
- **物件映射**: POCO (不使用 AutoMapper)

### 技術選擇理由
- ASP.NET Core 8.0: 企業級效能、完整的 .NET 生態
- SQL Server: 企業級可靠性、複雜查詢支援
- EF Core Code First: 版本控制友善、易於遷移
- 無 Redis: 此規模使用 IMemoryCache 足夠
- 無 AutoMapper: POCO 簡單直接
- 無 Minimal APIs: 傳統控制器更結構化

### 相依性
- 公司 WiAD/Active Directory 伺服器
- 公司 SMTP 郵件伺服器
- SQL Server 資料庫伺服器

## 憲章檢查 (Constitution Check)

### 適用原則

✅ **I. 代碼品質標準**
- 最大循環複雜度: 10
- 最小程式碼涵蓋率: 80%
- 所有公開 API 需要文件和示例
- 遵循 SOLID 原則

✅ **II. 測試卓越性**
- 實施 TDD 方法
- 單元測試所有業務邏輯
- 整合測試服務介面
- 端到端測試關鍵使用者流程

✅ **III. 使用者體驗一致性**
- 所有使用者介面必須使用繁體中文 (zh-TW)
- 響應式設計
- WCAG 2.1 AA 無障礙合規
- 統一錯誤處理和訊息

✅ **IV. 性能要求**
- 頁面載入時間 < 2 秒
- API 回應時間 (95th percentile) < 200ms
- 客戶端渲染 < 100ms

✅ **V. 安全標準**
- 安全的身份驗證和授權
- 定期安全稽核
- 輸入驗證和防衛
- OWASP Top 10 保護

✅ **VI. 文件語言標準**
- 所有規格和技術文件必須使用繁體中文
- API 文件使用繁體中文
- 錯誤訊息使用繁體中文
- 程式碼註釋使用繁體中文

## Phase 0: 研究與釐清

### 研究任務

#### 1. WiAD/LDAP 整合
**狀態**: ✅ 完成
- 決策：使用 System.DirectoryServices 整合 AD
- 方法：LDAP 協議，IMemoryCache 快取
- TTL：1 小時
- 詳見 `research.md`

#### 2. 郵件系統整合
**狀態**: ✅ 完成
- 決策：使用 System.Net.Mail + IHostedService
- 方法：後台服務佇列管理，資料庫持久化
- 重試機制：3 次重試
- 詳見 `research.md`

#### 3. 時段衝突檢測演算法
**狀態**: ✅ 完成
- 決策：SQL Server DATEDIFF 範圍查詢
- 方法：雙層驗證（資料庫 + 應用層）
- EF Core 效能最佳化
- 詳見 `research.md`

#### 4. 記憶體快取策略
**狀態**: ✅ 完成
- 決策：使用 IMemoryCache，無 Redis
- 策略：AD 快取 (1hr)、審查人員清單 (1hr)、預約資料 (15min)
- 詳見 `research.md`

#### 5. 認證與授權架構
**狀態**: ✅ 完成
- 決策：LDAP 驗證 + JWT token 無狀態認證
- 方法：RBAC (Role-Based Access Control)
- 詳見 `research.md`

#### 6. 構控制器架構
**狀態**: ✅ 完成
- 決策：傳統控制器模式 (ASP.NET Core Controllers)
- 理由：結構化、易於測試、避免 Minimal APIs 複雜性
- 詳見 `research.md`

### 研究輸出
✅ `research.md` - 包含 11 個 ASP.NET Core 技術決策和理由

## Phase 1: 設計與約定

### 1. 資料模型
**狀態**: ✅ 完成
**檔案**: `data-model.md`
**內容**:
- 5 個核心實體定義 (User, Appointment, LeaveSchedule, AppointmentHistory, NotificationLog)
- 完整的欄位、型別、約束說明
- 狀態轉換圖
- 資料庫初始化 SQL 指令
- 效能最佳化策略
- 快取策略

### 2. API 契約
**狀態**: ✅ 完成
**檔案**: `contracts/openapi.yaml`
**內容**:
- OpenAPI 3.0 規範
- 11 個主要端點定義
- 認證、月曆、預約、休假 4 個主題分類
- 完整的請求/回應範例
- 5 個核心資料結構定義

### 3. 快速開始指南
**狀態**: ✅ 完成
**檔案**: `quickstart.md`
**內容**:
- 環境設置 (Docker Compose 和本機)
- 專案結構說明
- 常用命令參考
- 常見問題解答
- 部署準備指南
- 憲章合規性檢查清單

## Phase 2: 任務分解

### 待分解領域

1. **後端基礎設施**
   - ASP.NET Core 專案初始化和依賴 NuGet 套件
   - SQL Server 資料庫連線設定
   - Entity Framework Core DbContext 設置
   - 初始遷移和資料庫建立
   - Serilog 結構化日誌設定
   - 全域例外處理中間件

2. **認證與授權**
   - System.DirectoryServices LDAP 整合
   - AD 帳號驗證和同步
   - JWT token 簽發和驗證
   - IMemoryCache LDAP 快取層
   - 角色型存取控制 (RBAC) 實現

3. **預約核心功能**
   - Appointment 控制器和服務層
   - 預約 CRUD 操作
   - 時段衝突驗證（SQL Server 查詢優化）
   - 休假檢查邏輯
   - 預約狀態轉換工作流
   - 業務驗證規則

4. **審查人員功能**
   - 預約接受/拒絕端點
   - 休假排程管理
   - 預約轉送和代理接受/拒絕
   - LeaveSchedule 控制器

5. **郵件通知系統**
   - IHostedService 後台服務實現
   - 通知範本管理
   - NotificationLog 記錄
   - SMTP 郵件發送
   - 失敗重試機制 (3 次)

6. **API 文件和驗證**
   - Swagger/OpenAPI 整合
   - 請求和回應驗證
   - 錯誤處理標準化
   - API 文件補充

7. **測試和品質保證**
   - 單元測試（xUnit）
   - 整合測試（測試資料庫）
   - 端到端測試（API 測試）
   - 效能測試和優化

8. **部署和文件**
   - Docker 容器化 (可選)
   - CI/CD 管線設定
   - 部署指南編寫
   - 營運手冊編寫

## 門檻檢查

- ✅ 規格已釐清 (5 個關鍵決策已解決)
- ✅ 研究已完成 (10 個技術決策已定義)
- ✅ 資料模型設計完成 (5 個實體完整定義)
- ✅ API 契約已定義 (OpenAPI 3.0 規範)
- ✅ 開發指南已完成 (快速開始指南)
- ✅ 憲章檢查通過 (所有 6 項原則都已遵循)

## 後續步驟

1. ✅ Phase 0 研究完成 → `research.md`
2. ✅ Phase 1 設計完成 → `data-model.md`, `contracts/`, `quickstart.md`
3. ⏳ Phase 2: 進行任務分解 → 產出 `tasks.md`

## 已產出檔案清單

| 檔案名 | 用途 | 狀態 |
|--------|------|------|
| `spec.md` | 功能規格 | ✅ 完成 |
| `plan.md` | 實作計畫 (本檔案) | ✅ 完成 |
| `research.md` | 技術決策研究 | ✅ 完成 |
| `data-model.md` | 資料庫設計 | ✅ 完成 |
| `contracts/openapi.yaml` | API 規範 | ✅ 完成 |
| `quickstart.md` | 開發指南 | ✅ 完成 |
| `tasks.md` | 任務分解 | ⏳ 待產出 |
| `checklists/requirements.md` | 規格檢查清單 | ✅ 完成 |