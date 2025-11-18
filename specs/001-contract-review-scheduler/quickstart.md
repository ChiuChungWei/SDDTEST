# 契約審查預約系統 - 快速開始指南

**日期**: 2025-11-18  
**目標讀者**: 開發人員

## 概述

本指南將帶您快速設置並運行契約審查預約系統的開發環境。

## 必要條件

確保您已安裝以下軟體：

- **Node.js**: 18.x 或更高版本
- **PostgreSQL**: 14 或更高版本
- **Redis**: 7 或更高版本
- **Docker** 和 **Docker Compose** (推薦用於簡化設置)
- **Git**: 2.x 或更高版本

## 開發環境設置

### 方式 1：使用 Docker Compose (推薦)

#### 1. 複製環境設定檔

```bash
cp .env.example .env
```

編輯 `.env` 檔案，設定以下變數：

```env
# 應用設定
NODE_ENV=development
PORT=3000
API_PORT=3001

# 資料庫設定
DB_HOST=db
DB_PORT=5432
DB_USER=admin
DB_PASSWORD=changeme
DB_NAME=contract_review

# Redis 設定
REDIS_HOST=redis
REDIS_PORT=6379

# WiAD 設定
LDAP_URL=ldap://your-ad-server:389
LDAP_BASE_DN=dc=company,dc=com
LDAP_USERNAME=your-ad-user
LDAP_PASSWORD=your-ad-password

# 郵件設定
SMTP_HOST=mail.company.com
SMTP_PORT=587
SMTP_USER=noreply@isn.co.jp
SMTP_PASSWORD=your-smtp-password
SMTP_FROM=noreply@isn.co.jp
```

#### 2. 啟動服務

```bash
docker-compose up -d
```

此指令將啟動：
- PostgreSQL 資料庫
- Redis 快取
- 後端 API 伺服器 (3001 埠)
- 前端開發伺服器 (3000 埠)

#### 3. 初始化資料庫

```bash
docker-compose exec api npm run migrate
```

### 方式 2：本機安裝

#### 1. 安裝依賴

```bash
# 後端依賴
cd backend
npm install

# 前端依賴
cd ../frontend
npm install
```

#### 2. 設定環境變數

在專案根目錄建立 `.env` 檔案，參照上方的環境變數範例。

#### 3. 啟動 PostgreSQL 和 Redis

```bash
# macOS (使用 Homebrew)
brew services start postgresql
brew services start redis

# Linux (Ubuntu/Debian)
sudo systemctl start postgresql
sudo systemctl start redis-server

# Windows (使用 Docker)
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=changeme postgres:14
docker run -d -p 6379:6379 redis:7
```

#### 4. 初始化資料庫

```bash
cd backend
npm run migrate
```

#### 5. 啟動開發伺服器

```bash
# 終端 1：後端
cd backend
npm run dev

# 終端 2：前端
cd frontend
npm start
```

## 專案結構

```
contract-review-scheduler/
├── backend/                    # 後端應用
│   ├── src/
│   │   ├── config/            # 設定檔
│   │   ├── controllers/       # 控制器
│   │   ├── models/            # 資料模型
│   │   ├── routes/            # 路由定義
│   │   ├── services/          # 業務邏輯
│   │   ├── middleware/        # 中介軟體
│   │   ├── utils/             # 工具函式
│   │   └── index.ts           # 應用入點
│   ├── tests/                 # 測試檔案
│   ├── migrations/            # 資料庫遷移
│   └── package.json
├── frontend/                   # 前端應用
│   ├── src/
│   │   ├── components/        # React 元件
│   │   ├── pages/             # 頁面
│   │   ├── services/          # API 服務
│   │   ├── store/             # Redux 狀態
│   │   ├── styles/            # 樣式檔案
│   │   ├── utils/             # 工具函式
│   │   └── App.tsx            # 應用根元件
│   ├── tests/                 # 測試檔案
│   └── package.json
├── docker-compose.yml         # Docker Compose 設定
├── .env.example               # 環境變數範例
└── README.md
```

## 常用命令

### 後端

```bash
cd backend

# 安裝依賴
npm install

# 開發模式執行
npm run dev

# 執行測試
npm test

# 執行程式碼涵蓋率分析
npm run test:coverage

# 建立生產版本
npm run build

# 資料庫遷移
npm run migrate
npm run migrate:rollback

# 程式碼 Linting
npm run lint
npm run lint:fix
```

### 前端

```bash
cd frontend

# 安裝依賴
npm install

# 開發模式執行
npm start

# 建立生產版本
npm run build

# 執行測試
npm test

# 程式碼 Linting
npm run lint
npm run lint:fix

# 執行 E2E 測試
npm run e2e
```

## API 文件

API 文件使用 OpenAPI 3.0 規範，位於 `contracts/openapi.yaml`。

### 檢視 API 文件

啟動應用後，可通過以下方式檢視 API 文件：

```
http://localhost:3001/api/docs
```

## 資料庫連接

### 使用 pgAdmin 管理 PostgreSQL

如果使用 Docker Compose，pgAdmin 會自動啟動在 5050 埠：

```
http://localhost:5050
```

登入認證 (預設)：
- Email: admin@admin.com
- Password: admin

### 連接資訊

- Host: db (在 Docker) 或 localhost (本機)
- Port: 5432
- Username: admin
- Password: changeme (或您在 `.env` 中設定的密碼)
- Database: contract_review

## 常見問題

### Q: 如何重置資料庫？

```bash
# 後端
cd backend
npm run migrate:rollback:all
npm run migrate
```

### Q: 如何檢查 Redis 連線？

```bash
redis-cli ping
# 應輸出：PONG
```

### Q: 如何檢查 PostgreSQL 連線？

```bash
psql -h localhost -U admin -d contract_review -c "SELECT 1;"
```

### Q: 如何除錯前端應用？

使用 Chrome DevTools：
1. 開啟瀏覽器開發者工具 (F12)
2. 切換到 Sources 選項卡
3. 設定斷點並檢查變數

### Q: 如何查看後端日誌？

```bash
# Docker
docker-compose logs -f api

# 本機
cd backend && npm run dev  # 日誌直接輸出到終端
```

## 測試

### 單元測試

```bash
# 後端
cd backend
npm test

# 前端
cd frontend
npm test
```

### 整合測試

```bash
cd backend
npm run test:integration
```

### 端到端測試

```bash
cd frontend
npm run e2e
```

### 程式碼涵蓋率

```bash
# 後端 (目標：80%)
cd backend
npm run test:coverage

# 前端
cd frontend
npm run test:coverage
```

## 效能測試

### 本機效能測試

```bash
cd backend
npm run test:performance
```

## 部署準備

### 建立生產版本

```bash
# 後端
cd backend
npm run build

# 前端
cd frontend
npm run build
```

生產版本檔案位於：
- 後端: `backend/dist/`
- 前端: `frontend/build/`

### 使用 Docker 部署

```bash
docker-compose -f docker-compose.prod.yml up -d
```

## 憲章合規性檢查清單

開發時請確保符合以下要求：

- ✅ 所有使用者介面文字使用繁體中文
- ✅ API 錯誤訊息使用繁體中文
- ✅ 程式碼註釋使用繁體中文
- ✅ 單元測試涵蓋率 >= 80%
- ✅ 函式循環複雜度 <= 10
- ✅ API 回應時間 < 200ms
- ✅ 頁面載入時間 < 2 秒
- ✅ 實施 WCAG 2.1 AA 無障礙標準
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