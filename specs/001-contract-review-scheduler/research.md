# 契約審查預約系統 - 研究與技術決策

**日期**: 2025-11-18 (更新: ASP.NET Core 技術棧)  
**狀態**: 完成
**實作語言**: C# + ASP.NET Core 8.0

## 技術棧選擇

### 決策
使用 **ASP.NET Core 8.0 Web API** 搭配 **SQL Server** 資料庫，**EF Core Code First** 開發方式

### 理由
- ASP.NET Core 8.0 是微軟企業級框架，效能優異
- SQL Server 提供企業級資料庫功能和可靠性
- EF Core Code First 支援版本控制友善的模式遷移
- 原生 LDAP 支援 (System.DirectoryServices)
- 內建的相依性注入和中介軟體管理
- 全面的日誌記錄和診斷功能

### 評估的替代方案
- ❌ Node.js Express - 效能不如 ASP.NET Core，企業支援力度不足
- ❌ Python FastAPI - 資料型別安全性不如 C#
- ✅ ASP.NET Core 8.0 (選定)
- ❌ Java Spring Boot - 公司技術棧偏向微軟生態

### 關鍵決策
- **資料庫**: SQL Server 2019+
- **ORM**: Entity Framework Core (Code First)
- **DTO 對映**: POCO (不使用 AutoMapper)
- **快取**: 內存快取 (IMemoryCache，不使用 Redis)
- **郵件**: System.Net.Mail + 後台服務
- **認證**: LDAP (System.DirectoryServices)

---

## 1. WiAD 認證整合

### 決策
使用 **System.DirectoryServices** 進行 Active Directory LDAP 認證

### 理由
- System.DirectoryServices 是 .NET Framework 原生支援
- 無需第三方套件，直接使用 Windows/AD 基礎設施
- ASP.NET Core 完全支援
- 提供完整的 LDAP 功能

### 評估的替代方案
- ❌ ldapjs - Node.js 套件，不適用於 ASP.NET Core
- ✅ System.DirectoryServices (選定)
- ❌ Azure AD - 額外的雲端成本
- ❌ Kerberos 單一登入 - 超出此專案需求

### 關鍵實作細節
- 使用 `DirectoryEntry` 和 `DirectorySearcher` 連接 AD
- 郵件生成規則：`{AD帳號}@isn.co.jp`
- 快取 AD 查詢結果以改善效能 (IMemoryCache，TTL: 1 小時)
- 實作完善的異常處理和重試機制
- 建立服務類別 `ILdapService` 封裝 LDAP 邏輯

---

## 2. 郵件系統整合

### 決策
使用 **System.Net.Mail (SmtpClient)** 搭配背景服務隊列進行郵件發送

### 理由
- System.Net.Mail 是 .NET 原生套件，無外部相依
- 支援 SMTP 協議（公司郵件系統標準）
- ASP.NET Core 背景服務可實作郵件隊列機制
- 資料庫記錄可追蹤郵件發送狀態

### 評估的替代方案
- ❌ Nodemailer - Node.js 套件，不適用於 ASP.NET Core
- ✅ System.Net.Mail + 背景服務 (選定)
- ❌ SendGrid/AWS SES - 公司政策要求使用內部郵件系統
- ❌ 手工撰寫 SMTP 客戶端 - 重複造輪

### 關鍵實作細節
- 建立 `IEmailService` 介面進行郵件發送
- 實作後台服務 `EmailQueueService` 管理郵件隊列
- 在 `NotificationLog` 表中記錄所有郵件發送
- 重試機制：最多重試 3 次，間隔 5 分鐘
- 所有郵件內容使用繁體中文範本
- 使用 `HostedService` 實作背景郵件處理

---

## 3. 資料庫設計

### 決策
使用 **SQL Server** 搭配 **Entity Framework Core Code First**

### 理由
- SQL Server 提供企業級功能（事務、索引、執行計畫最佳化）
- EF Core Code First 與版本控制友善
- 支援複雜查詢和 LINQ
- 原生的日期時間和 Unicode 支援

### 評估的替代方案
- ❌ PostgreSQL - 雖然功能強大，但公司使用 SQL Server
- ✅ SQL Server (選定)
- ❌ MySQL - 事務支援和複雜查詢能力不如 SQL Server
- ❌ SQLite - 適合開發，不適合生產

### 關鍵實作細節
- 使用 DbContext 定義所有實體
- Code First 遷移進行版本控制
- 建立適當的索引支援查詢效能
- 使用資料庫約束確保資料完整性
- 建立 Fluent API 配置進階映射

---

## 4. 時段衝突檢測演算法

### 決策
使用 **SQL Server 時間重疊查詢** 搭配 **應用層驗證** 的雙層驗證模式

### 理由
- SQL Server 的 DATEDIFF 函式高效處理時間範圍比較
- 應用層驗證提供快速反饋
- 使用交易保證原子性，防止並發衝突

### 技術選擇
- **SQL 查詢**: 
  ```sql
  SELECT COUNT(*) FROM Appointments 
  WHERE ReviewerId = @ReviewerId 
    AND AppointmentDate = @Date
    AND NOT (TimeEnd <= @TimeStart OR TimeStart >= @TimeEnd)
    AND Status NOT IN ('Rejected', 'Cancelled');
  ```
- **資料庫索引**: (ReviewerId, AppointmentDate, TimeStart, TimeEnd)
- **應用層檢查**: LINQ to EF 進行查詢
- **交易隔離等級**: SERIALIZABLE 進行關鍵操作

### 關鍵實作細節
- 在 `AppointmentService` 建立衝突檢測方法
- 使用 `DbContext.Database.BeginTransaction` 保證原子性
- 提供清晰的衝突錯誤訊息給用戶端

---

## 5. ORM 和物件映射策略

### 決策
使用 **Entity Framework Core** 搭配 **POCO** (Plain Old CLR Object)，不使用 AutoMapper

### 理由
- POCO 簡單直接，易於版本控制
- EF Core 自動跟蹤變更
- 大幅減少外部相依
- 手動映射明確且易於除錯

### 評估的替代方案
- ❌ AutoMapper - 增加複雜度和外部相依
- ✅ POCO 直接映射 (選定)
- ❌ OData - 超出此專案需求

### 關鍵實作細節
- 定義 POCO 模型對應資料庫實體
- API 回應 DTO 與實體保持一致結構
- 自訂映射邏輯寫在 Service 層
- 防止 Lazy Loading 問題，使用 `.Include()` 顯式載入

---

## 6. 後台服務與隊列管理

### 決策
使用 **ASP.NET Core Hosted Services** 進行背景任務管理（郵件、定期清理）

### 理由
- Hosted Services 是 ASP.NET Core 原生功能
- 無需外部隊列系統 (如 RabbitMQ、Kafka)
- 適合此規模的應用
- 簡化部署和運維

### 評估的替代方案
- ❌ Hangfire - 額外開源相依，此規模不必要
- ❌ RabbitMQ/Kafka - 過於複雜
- ✅ Hosted Services (選定)
- ❌ 執行緒池 - 不夠可靠

### 關鍵實作細節
- 實作 `IHostedService` 進行郵件處理
- 建立 `EmailQueue` 表儲存待發送郵件
- 背景服務定期檢查並發送郵件
- 異常處理和重試機制

---

## 7. API 設計與控制器

### 決策
使用 **傳統控制器模式** (Controllers)，而不使用 Minimal APIs

### 理由
- 傳統控制器提供更結構化的組織方式
- 便於團隊協作和程式碼標準化
- 更容易實作橫切關注點 (認證、授權、日誌)
- 更適合複雜的業務邏輯

### 評估的替代方案
- ❌ Minimal APIs - 適合簡單 API，不適合此規模
- ✅ 傳統控制器 (選定)
- ❌ GraphQL - 增加複雜度

### 關鍵實作細節
- 建立 ApiController 基底類別
- 實作全域異常處理中介軟體
- 標準化 API 回應格式
- 使用 Attribute-based 路由

---

## 8. 快取策略

### 決策
使用 **System.Runtime.Caching (IMemoryCache)** 進行應用層快取

### 理由
- IMemoryCache 是 ASP.NET Core 原生功能
- 不需要 Redis 外部依賴
- 適合此規模應用的快取需求
- 簡化部署

### 評估的替代方案
- ❌ Redis - 不必要的複雜度
- ✅ IMemoryCache (選定)
- ❌ 分散式快取 - 此規模不需要

### 關鍵實作細節
- 快取 AD 查詢結果 (TTL: 1 小時)
- 快取審查人員清單 (TTL: 1 小時)
- 快取預約狀態 (TTL: 15 分鐘)
- 預約變更時主動失效快取

---

## 9. 認證與授權

### 決策
使用 **基於角色的存取控制 (RBAC)** 搭配 **JWT 令牌**

### 理由
- JWT 無狀態且易於擴展
- RBAC 簡單直接符合此業務需求
- ASP.NET Core 原生支援

### 關鍵實作細節
- 使用 `[Authorize]` 和 `[Authorize(Roles = "...")]` 特性
- 實作自訂授權處理程式
- 登入時簽發 JWT 令牌
- 實作 LDAP 驗證中介軟體

---

## 10. 日誌和監控

### 決策
使用 **Serilog** 進行結構化日誌記錄

### 理由
- Serilog 是業界標準的 .NET 日誌庫
- 支援結構化日誌（便於分析）
- 支援多個 Sink (檔案、資料庫、雲端)
- 與 ASP.NET Core 整合良好

### 關鍵實作細節
- 配置 Serilog 寫入檔案和事件日誌
- 記錄所有 API 呼叫
- 記錄認證嘗試
- 記錄郵件發送狀態

---

## 11. 效能最佳化

### 決策
使用 **SQL 查詢最佳化** + **應用層快取** + **非同步操作**

### 理由
- 達成 API 回應時間 < 200ms 目標
- 減少資料庫查詢負擔

### 技術選擇
- **資料庫層**: 適當的索引、執行計畫最佳化
- **應用層**: IMemoryCache 進行熱點資料快取
- **非同步**: 所有 I/O 操作使用 async/await
- **批次查詢**: 使用 LINQ 投影減少傳輸數據量

### 關鍵實作細節
- 在關鍵路徑使用 `.AsNoTracking()` 提高查詢速度
- 使用 `.Include()` 和 `.ThenInclude()` 避免 N+1 查詢
- 實作查詢分頁限制
- 監控慢查詢並最佳化

---

## 總結

所有 11 項主要技術決策已完成，針對 ASP.NET Core 8.0 生態最佳化：

| 決策項目 | 選擇 | 理由 |
|---------|------|------|
| 框架 | ASP.NET Core 8.0 | 企業級效能 |
| 資料庫 | SQL Server | 企業級功能 |
| ORM | EF Core Code First | 版本控制友善 |
| 物件映射 | POCO | 簡單直接 |
| 快取 | IMemoryCache | 無外部相依 |
| 後台服務 | Hosted Services | 原生支援 |
| 認證 | LDAP + JWT | 標準做法 |
| API 模式 | 控制器 | 結構化 |
| 日誌 | Serilog | 業界標準 |
| 效能 | 多層最佳化 | 達成目標 |

實作團隊可基於上述決策進行 Phase 1 設計工作。

---

## 4. 時段衝突檢測演算法

### 決策
使用 **資料庫級別的範圍查詢** 搭配 **應用層驗證** 的雙層驗證模式

### 理由
- 資料庫層驗證確保數據完整性，防止並發衝突
- 應用層驗證提供快速反饋和使用者友善的錯誤訊息
- 使用 PostgreSQL 範圍類型 (range type) 提高查詢效率
- 減少資料庫往返次數

### 技術選擇
- **資料庫索引**: 使用 GiST 索引於時段範圍欄位
- **SQL 查詢**: 
  ```sql
  SELECT * FROM appointments 
  WHERE reviewer_id = $1 
    AND appointment_date = $2
    AND appointment_time && $3  -- 範圍重疊檢查
    AND status != 'rejected'
  LIMIT 1;
  ```
- **應用層檢查**: 轉換用戶選擇的時段為時間戳記，比對查詢結果

### 關鍵實作細節
- 在 appointments 表建立複合索引: (reviewer_id, appointment_date, appointment_time)
- 實作原子性的插入-檢查操作避免 Race Condition
- 使用 PostgreSQL 事務隔離等級 SERIALIZABLE 進行關鍵操作
- 提供清晰的衝突錯誤訊息給使用者

---

## 5. 資料持久化策略

### 決策
使用 **PostgreSQL** 作為主要資料庫，搭配 **Redis** 進行快取

### 理由
- PostgreSQL 提供 ACID 保證，適合交易型應用
- 支援複雜查詢、事務和範圍類型
- Redis 快取 AD 查詢和熱點資料（審查人員清單、時段資訊）
- 改善 API 回應時間，達成 < 200ms 的目標

### 評估的替代方案
- ❌ MongoDB - 對於高度結構化的預約資料不如關聯式資料庫
- ❌ MySQL - PostgreSQL 更好的範圍查詢支援
- ✅ PostgreSQL + Redis (選定)
- ❌ 單純記憶體資料庫 - 無法滿足資料持久性需求

### 關鍵實作細節
- 使用 Sequelize 或 TypeORM 作為 ORM
- 快取層使用 Redis，TTL 設定為 5 分鐘
- 預約建立/更改時主動失效相關快取
- 定期備份 PostgreSQL 資料

---

## 6. 前端框架選擇

### 決策
使用 **React + TypeScript** 搭配 **Material-UI** 元件庫

### 理由
- React 是業界標準的前端框架，社群龐大
- TypeScript 提供型別安全，減少執行時錯誤
- Material-UI 提供專業的元件庫和繁體中文支援
- Redux Toolkit 簡化狀態管理

### 評估的替代方案
- ❌ Vue.js - 社群較小，公司技術棧偏 React
- ❌ Angular - 過重，不適合此規模專案
- ✅ React + TypeScript (選定)
- ❌ 原生 JavaScript - 缺乏型別安全和元件化

### 關鍵實作細節
- 使用 Create React App 或 Vite 初始化
- Material-UI v5+ 支援繁體中文
- Redux Toolkit 管理全域狀態
- React Router v6 進行路由管理

---

## 7. API 設計風格

### 決策
使用 **RESTful API** 搭配 **JSON** 資料格式

### 理由
- RESTful 是業界標準，與公司其他系統相容
- JSON 是現代 Web API 的標準格式
- 易於文件化和測試
- 前後端可清晰分離

### 評估的替代方案
- ❌ GraphQL - 此專案的查詢複雜度不足以證明其價值
- ✅ RESTful API (選定)
- ❌ RPC 風格 - 過時，缺乏標準化

### 關鍵實作細節
- 使用 OpenAPI 3.0 規格文件化 API
- 統一的錯誤回應格式
- HTTP 狀態碼正確使用 (200, 201, 400, 401, 404, 500)
- API 版本控制準備 (/api/v1/*)

---

## 8. 測試策略

### 決策
實施 **分層測試** 方法：單元測試 + 整合測試 + 端到端測試

### 理由
- 達成 80% 程式碼涵蓋率目標
- 實施 TDD 開發方法論
- 早期發現缺陷

### 技術選擇
- **單元測試**: Jest (前後端)
- **整合測試**: Supertest (API) + React Testing Library (元件)
- **端到端測試**: Cypress (關鍵使用者流程)
- **測試覆蓋**: Istanbul/nyc 監測程式碼涵蓋率

### 關鍵實作細節
- 目標涵蓋率: 80%
- Mock 外部依賴 (WiAD, SMTP 伺服器)
- 建立測試資料固定器 (fixtures)
- 在 CI/CD 中自動執行測試

---

## 9. 部署與運維

### 決策
使用 **Docker** 容器化部署，搭配 **Docker Compose** 進行本機開發

### 理由
- Docker 確保開發、測試、生產環境一致性
- 簡化依賴管理
- 易於擴展和更新

### 關鍵實作細節
- 前後端分離的 Dockerfile
- docker-compose.yml 包含 PostgreSQL、Redis、應用服務
- 環境變數管理 (.env 檔案)
- 健康檢查機制

---

## 10. 效能最佳化

### 決策
實施 **多層快取** + **資料庫查詢最佳化**

### 理由
- 達成 API 回應時間 < 200ms 目標
- 減少資料庫查詢負擔

### 技術選擇
- **HTTP 快取**: 利用 ETags 和 Cache-Control 標頭
- **應用層快取**: Redis 快取熱點資料
- **資料庫層快取**: 適當的索引策略
- **前端最佳化**: 代碼分割、懶加載、圖片最佳化

### 關鍵實作細節
- 月曆資料快取 (TTL: 5 分鐘)
- 審查人員清單快取 (TTL: 1 小時)
- API 分頁限制 (預設 50 項)
- 監控 API 回應時間

---

## 總結

所有主要技術決策已完成，無遺留的關鍵澄清事項。實作團隊可基於上述決策進行 Phase 1 設計工作。