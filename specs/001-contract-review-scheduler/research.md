# 契約審查預約系統 - 研究與技術決策

**日期**: 2025-11-18  
**狀態**: 完成

## 1. WiAD 認證整合

### 決策
使用 **Active Directory (LDAP)** 協議透過公司 WiAD 系統進行身份驗證

### 理由
- WiAD 是公司標準的身份識別系統
- LDAP 是業界標準的 AD 整合協議
- Node.js 有成熟的 `ldapjs` 或 `active-directory` 套件支援
- 無需額外的 SSO 基礎設施投資

### 評估的替代方案
- ❌ OAuth2/OpenID Connect - 公司 WiAD 不支援
- ❌ SAML - 超過此專案的複雜度需求
- ✅ LDAP (選定)
- ❌ 自訂帳號系統 - 重複工作，違反公司政策

### 關鍵實作細節
- 使用 `ldapjs` npm 套件連接 AD
- 郵件生成規則：`{AD帳號}@isn.co.jp`
- 快取 AD 查詢結果以改善效能 (TTL: 1 小時)
- 實作完善的錯誤處理和重試機制

---

## 2. 郵件系統整合

### 決策
使用 **Nodemailer** 搭配公司 SMTP 伺服器進行郵件發送

### 理由
- Nodemailer 是 Node.js 最流行的郵件發送套件
- 支援 SMTP 協議（公司郵件系統標準）
- 內建郵件佇列和重試機制支援
- 支援 HTML 郵件範本

### 評估的替代方案
- ❌ SendGrid/AWS SES - 公司政策要求使用內部郵件系統
- ❌ 直接 SMTP - 缺乏錯誤處理和重試邏輯
- ✅ Nodemailer (選定)
- ❌ 手工撰寫 SMTP 客戶端 - 重複造輪

### 關鍵實作細節
- 設定檔案存放 SMTP 伺服器位址、帳號、密碼
- 使用 `bull` 或 `node-queue` 管理郵件佇列
- 實作失敗重試機制 (最多 3 次)
- 所有郵件範本使用繁體中文
- 記錄所有發送的郵件到資料庫稽核

---

## 3. 月曆 UI 元件

### 決策
使用 **React Big Calendar** 搭配自訂時段顯示

### 理由
- React Big Calendar 是 React 生態中最成熟的月曆元件
- 原生支援月、週、日檢視
- 支援事件（預約）的自訂渲染
- 效能好，支援大量事件
- 活躍的社群和文件

### 評估的替代方案
- ❌ react-calendar - 功能較基礎，月曆展示能力不足
- ❌ full-calendar - 過重，提供超出需求的功能
- ✅ React Big Calendar (選定)
- ❌ 自訂 HTML 月曆 - 時間成本高，維護困難

### 關鍵實作細節
- 自訂事件渲染器顯示預約狀態顏色
- 已確認預約顯示黃色標記
- 待確認預約顯示灰色標記
- 實作自訂工具提示顯示時段詳情
- 支援月曆點擊開啟預約對話

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