# 前端與後端整合指南

**日期**: 2025-11-18  
**版本**: 1.0  
**架構**: REST API + JWT 認證  

---

## 目錄

1. [架構概觀](#架構概觀)
2. [API 契約](#api-契約)
3. [認證流程](#認證流程)
4. [資料同步](#資料同步)
5. [錯誤處理](#錯誤處理)
6. [效能最佳化](#效能最佳化)
7. [開發協調](#開發協調)

---

## 架構概觀

```
┌─────────────────────┐
│  React 前端應用      │
│  (Port 5173)        │
├─────────────────────┤
│ - 使用者界面        │
│ - 表單驗證          │
│ - 狀態管理 (Context)│
│ - 路由導航          │
└──────────┬──────────┘
           │
      HTTP/REST
    (JSON + JWT)
           │
┌──────────▼──────────┐
│ ASP.NET Core API    │
│ (Port 5000)         │
├─────────────────────┤
│ - 業務邏輯          │
│ - 資料驗證          │
│ - 資料庫操作        │
│ - LDAP 認證         │
└─────────────────────┘
           │
    SQL Server 2019+
```

### 分離架構優勢

| 優勢 | 說明 |
|------|------|
| **獨立部署** | 前後端可分別更新，無相依性 |
| **團隊分工** | 前端 / 後端 / QA 並行開發 |
| **可擴展性** | 前後端可獨立擴展 |
| **語言自由** | 前端用 React，後端用 C# 最適合各自 |
| **測試獨立** | API 可獨立測試，前端元件也可獨立測試 |

---

## API 契約

### 基本設定

| 項目 | 值 |
|------|-----|
| **基礎 URL** | `http://localhost:5000/api` (開發) |
| **基礎 URL** | `https://api.example.com/api` (生產) |
| **內容型態** | `application/json` |
| **認證** | `Authorization: Bearer <JWT_TOKEN>` |
| **超時** | 10 秒 |

### HTTP 狀態碼

| 代碼 | 意義 | 處理方式 |
|-----|------|---------|
| **200** | 成功 | 解析回應資料 |
| **201** | 已建立 | 資源已建立 |
| **204** | 無內容 | 操作成功，無回應體 |
| **400** | 請求錯誤 | 顯示驗證錯誤訊息 |
| **401** | 未授權 | 重導至登入頁面 |
| **403** | 禁止存取 | 顯示權限不足錯誤 |
| **404** | 未找到 | 顯示資源不存在 |
| **409** | 衝突 | 資料版本衝突，重新載入 |
| **500** | 伺服器錯誤 | 顯示通用錯誤訊息 |

---

### 11 個核心 API 端點

#### 1. 認證相關

##### 登入
```
POST /api/auth/login
Content-Type: application/json

Request:
{
  "adAccount": "user@company.com",
  "password": "password123"
}

Response (200 OK):
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "uuid",
    "adAccount": "user@company.com",
    "name": "王小明",
    "email": "user@company.com",
    "role": "applicant"
  }
}
```

##### 驗證 Token
```
POST /api/auth/verify
Authorization: Bearer <TOKEN>

Response (200 OK):
{
  "isValid": true,
  "expiresAt": "2025-12-18T10:30:00Z"
}
```

#### 2. 預約相關

##### 建立預約
```
POST /api/appointments
Authorization: Bearer <TOKEN>
Content-Type: application/json

Request:
{
  "objectName": "合約名稱",
  "timeStart": "2025-12-20T09:00:00Z",
  "timeEnd": "2025-12-20T10:00:00Z",
  "description": "合約內容描述",
  "attachments": ["file_url_1", "file_url_2"]
}

Response (201 Created):
{
  "id": "uuid",
  "objectName": "合約名稱",
  "status": "pending",
  "createdAt": "2025-12-18T10:30:00Z"
}
```

##### 取得預約列表
```
GET /api/appointments?status=pending&page=1&pageSize=10
Authorization: Bearer <TOKEN>

Response (200 OK):
{
  "data": [
    {
      "id": "uuid",
      "objectName": "合約1",
      "status": "pending",
      "timeStart": "2025-12-20T09:00:00Z",
      "timeEnd": "2025-12-20T10:00:00Z",
      "applicant": "王小明",
      "createdAt": "2025-12-18T10:30:00Z"
    }
  ],
  "total": 42,
  "page": 1,
  "pageSize": 10
}
```

##### 取得預約詳情
```
GET /api/appointments/{id}
Authorization: Bearer <TOKEN>

Response (200 OK):
{
  "id": "uuid",
  "objectName": "合約名稱",
  "status": "pending",
  "timeStart": "2025-12-20T09:00:00Z",
  "timeEnd": "2025-12-20T10:00:00Z",
  "description": "合約內容",
  "applicantId": "uuid",
  "applicantName": "王小明",
  "reviewerId": null,
  "reviewerName": null,
  "attachments": [
    {
      "id": "uuid",
      "url": "https://...",
      "fileName": "contract.pdf"
    }
  ],
  "history": [
    {
      "action": "created",
      "timestamp": "2025-12-18T10:30:00Z",
      "performedBy": "王小明"
    }
  ]
}
```

##### 確認預約
```
PATCH /api/appointments/{id}/confirm
Authorization: Bearer <TOKEN>
Content-Type: application/json

Response (204 No Content)
```

##### 拒絕預約
```
PATCH /api/appointments/{id}/reject
Authorization: Bearer <TOKEN>
Content-Type: application/json

Request:
{
  "rejectionReason": "時間衝突"
}

Response (204 No Content)
```

#### 3. 異議相關

##### 提交異議
```
POST /api/appointments/{appointmentId}/objections
Authorization: Bearer <TOKEN>
Content-Type: application/json

Request:
{
  "reason": "不同意預約時間",
  "proposedTimeStart": "2025-12-21T09:00:00Z",
  "proposedTimeEnd": "2025-12-21T10:00:00Z"
}

Response (201 Created):
{
  "id": "uuid",
  "appointmentId": "uuid",
  "status": "pending",
  "createdAt": "2025-12-18T10:30:00Z"
}
```

##### 取得異議列表
```
GET /api/objections?appointmentId={id}
Authorization: Bearer <TOKEN>

Response (200 OK):
{
  "data": [
    {
      "id": "uuid",
      "appointmentId": "uuid",
      "reason": "不同意預約時間",
      "proposedTimeStart": "2025-12-21T09:00:00Z",
      "status": "pending",
      "createdAt": "2025-12-18T10:30:00Z"
    }
  ]
}
```

#### 4. 休假排程相關

##### 建立休假
```
POST /api/leave-schedules
Authorization: Bearer <TOKEN>
Content-Type: application/json

Request:
{
  "startDate": "2025-12-24",
  "endDate": "2025-12-26",
  "reason": "聖誕假期"
}

Response (201 Created):
{
  "id": "uuid",
  "startDate": "2025-12-24",
  "endDate": "2025-12-26",
  "status": "active"
}
```

##### 取得休假列表
```
GET /api/leave-schedules
Authorization: Bearer <TOKEN>

Response (200 OK):
{
  "data": [
    {
      "id": "uuid",
      "startDate": "2025-12-24",
      "endDate": "2025-12-26",
      "reason": "聖誕假期",
      "createdAt": "2025-12-18T10:30:00Z"
    }
  ]
}
```

---

## 認證流程

### JWT Token 生命週期

```
使用者登入
    ↓
驗證 LDAP 認證
    ↓
生成 JWT Token (有效期 24 小時)
    ↓
返回 Token + 使用者資訊
    ↓
前端儲存在 localStorage
    ↓
後續請求在 Header 帶上 Token
    ↓
後端驗證 Token 簽名 + 過期時間
    ↓
若有效，繼續處理
若無效，回應 401 Unauthorized
    ↓
前端捕獲 401，導向登入頁面
```

### 前端實作

#### 1. 建立 API 客戶端 (Axios)

`src/api/client.ts`:

```typescript
import axios from 'axios';

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
});

// 請求攔截器：自動帶上 Token
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 回應攔截器：處理 401 Unauthorized
client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token 過期或無效
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default client;
```

#### 2. 建立 Auth Context

`src/contexts/AuthContext.tsx`:

```typescript
import { createContext, useContext, useState, useEffect } from 'react';
import client from '../api/client';

interface User {
  id: string;
  adAccount: string;
  name: string;
  email: string;
  role: 'applicant' | 'reviewer';
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  login: (adAccount: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // 初始化：檢查是否有既存 Token
  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('authToken');
    
    if (storedUser && storedToken) {
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, []);

  const login = async (adAccount: string, password: string) => {
    try {
      const response = await client.post('/auth/login', {
        adAccount,
        password,
      });

      const { token, user } = response.data;

      // 儲存 Token 和使用者資訊
      localStorage.setItem('authToken', token);
      localStorage.setItem('user', JSON.stringify(user));

      setUser(user);
    } catch (error) {
      console.error('登入失敗:', error);
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        loading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

---

## 資料同步

### 1. 預約列表取得

#### 前端

`src/api/appointments.ts`:

```typescript
import client from './client';

interface AppointmentListParams {
  status?: 'pending' | 'confirmed' | 'rejected';
  page?: number;
  pageSize?: number;
}

export const getAppointments = async (params: AppointmentListParams) => {
  const response = await client.get('/appointments', { params });
  return response.data;
};

export const getAppointmentById = async (id: string) => {
  const response = await client.get(`/appointments/${id}`);
  return response.data;
};

export const createAppointment = async (data: any) => {
  const response = await client.post('/appointments', data);
  return response.data;
};
```

`src/hooks/useAppointments.ts`:

```typescript
import { useState, useEffect } from 'react';
import { getAppointments, AppointmentListParams } from '../api/appointments';

export const useAppointments = (params: AppointmentListParams) => {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAppointments = async () => {
      try {
        setLoading(true);
        const data = await getAppointments(params);
        setAppointments(data.data);
      } catch (err) {
        setError('取得預約列表失敗');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchAppointments();
  }, [params.status, params.page]);

  return { appointments, loading, error };
};
```

#### 後端

`Controllers/AppointmentsController.cs`:

```csharp
[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AppointmentDto>>> GetAppointments(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var appointments = await _appointmentService.GetAppointmentsAsync(
            status, page, pageSize);
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetAppointmentById(string id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
            return NotFound();
        return Ok(appointment);
    }
}
```

### 2. 樂觀更新 (Optimistic Update)

當使用者提交表單時，前端應立即更新 UI，然後在後台發送請求。如果成功，資料已同步；如果失敗，應回復到舊狀態。

```typescript
const [appointment, setAppointment] = useState(appointmentData);

const confirmAppointment = async () => {
  // 樂觀更新：立即更新 UI
  const previousAppointment = appointment;
  setAppointment({ ...appointment, status: 'confirmed' });

  try {
    // 發送請求
    await client.patch(`/appointments/${appointment.id}/confirm`);
  } catch (error) {
    // 失敗時回復
    setAppointment(previousAppointment);
    alert('確認失敗，請重試');
  }
};
```

### 3. 實時通知 (SignalR - 可選)

若需要實時推送通知，可使用 SignalR:

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5000/hubs/notifications', {
    accessTokenFactory: () => localStorage.getItem('authToken') || '',
  })
  .withAutomaticReconnect()
  .build();

connection.on('AppointmentUpdated', (appointmentId: string) => {
  // 重新載入預約列表
  fetchAppointments();
});

connection.start().catch((err) => console.error(err));
```

---

## 錯誤處理

### 後端錯誤回應格式

```json
{
  "statusCode": 400,
  "message": "驗證失敗",
  "errors": [
    {
      "field": "objectName",
      "code": "REQUIRED",
      "message": "契約物件名稱為必填"
    },
    {
      "field": "timeEnd",
      "code": "INVALID_RANGE",
      "message": "結束時間必須晚於開始時間"
    }
  ]
}
```

### 前端錯誤處理

`src/api/errorHandler.ts`:

```typescript
export interface ApiError {
  statusCode: number;
  message: string;
  errors: Array<{
    field?: string;
    code: string;
    message: string;
  }>;
}

export const handleApiError = (error: any): ApiError => {
  if (error.response?.data) {
    return error.response.data as ApiError;
  }

  return {
    statusCode: 500,
    message: '未知錯誤',
    errors: [{ code: 'UNKNOWN', message: '發生未預期的錯誤' }],
  };
};

export const getFieldError = (
  errors: ApiError['errors'],
  fieldName: string
): string | null => {
  const error = errors.find((e) => e.field === fieldName);
  return error?.message || null;
};
```

`src/components/Forms/AppointmentForm.tsx`:

```typescript
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { handleApiError, getFieldError } from '../../api/errorHandler';

const AppointmentForm = () => {
  const handleSubmit = async (values: any) => {
    try {
      await createAppointment(values);
      alert('預約建立成功');
    } catch (error) {
      const apiError = handleApiError(error);
      
      // 從後端回應填入表單錯誤
      const fieldError = getFieldError(apiError.errors, 'objectName');
      if (fieldError) {
        // Formik 會自動顯示該欄位錯誤
      }
    }
  };

  return (
    <Formik
      initialValues={{
        objectName: '',
        timeStart: '',
        timeEnd: '',
      }}
      validationSchema={Yup.object({
        objectName: Yup.string().required('必填'),
        timeStart: Yup.string().required('必填'),
        timeEnd: Yup.string().required('必填'),
      })}
      onSubmit={handleSubmit}
    >
      <Form>
        <Field name="objectName" />
        <ErrorMessage name="objectName" component="div" />
      </Form>
    </Formik>
  );
};
```

---

## 效能最佳化

### 1. API 請求快取

```typescript
const appointmentCache = new Map<string, any>();

export const getAppointmentById = async (id: string) => {
  // 檢查快取
  if (appointmentCache.has(id)) {
    return appointmentCache.get(id);
  }

  // 發送請求
  const response = await client.get(`/appointments/${id}`);
  
  // 儲存快取
  appointmentCache.set(id, response.data);
  
  return response.data;
};
```

### 2. 虛擬化長列表

使用 React Window 或 React Virtual 優化大量資料的渲染:

```typescript
import { FixedSizeList } from 'react-window';

const AppointmentList = ({ appointments }: Props) => {
  const Row = ({ index, style }: any) => (
    <div style={style}>
      <AppointmentCard appointment={appointments[index]} />
    </div>
  );

  return (
    <FixedSizeList
      height={600}
      itemCount={appointments.length}
      itemSize={100}
      width="100%"
    >
      {Row}
    </FixedSizeList>
  );
};
```

### 3. 分頁而非一次載入所有資料

已在 API 設計中實現 (page, pageSize 參數)

### 4. 減少 API 呼叫

使用 GraphQL 或 OData 允許前端只請求需要的欄位:

```
GET /api/appointments?$select=id,objectName,status&$filter=status eq 'pending'
```

---

## 開發協調

### 開發週期

| 階段 | 後端 | 前端 | 說明 |
|------|------|------|------|
| **第 1 週** | API 架構設計 | 元件層級設計 | 並行工作 |
| **第 2 週** | 認證 API 實作 | 登入表單 | 等待 API |
| **第 3 週** | 預約 API 實作 | 預約表單 | 等待 API |
| **第 4 週** | 異議 API 實作 | 異議界面 | 等待 API |
| **第 5 週** | 測試 + 修復 | 整合測試 | 協調修復 |
| **第 6 週** | 部署準備 | 部署準備 | 端到端測試 |
| **第 7 週** | 上線 | 上線 | 監控 + 支援 |

### Git 工作流程

#### 分支策略

```
main (生產)
  ├── develop (開發)
  │   ├── feature/backend-auth (後端認證)
  │   ├── feature/frontend-login (前端登入)
  │   └── bugfix/api-error (修復)
  └── hotfix/security-patch (緊急修復)
```

#### 提交訊息慣例

```
type(scope): subject

[optional body]

[optional footer]
```

範例:

```
feat(backend): 實作 JWT 認證 API

- 新增 AuthController 登入端點
- 新增 JWT token 生成邏輯
- 配置 LDAP 認證

Closes #42
```

```
feat(frontend): 實作登入表單

- 新增 LoginForm 元件
- 整合 AuthContext
- 新增表單驗證

Depends-On: backend-auth
```

### API 文件同步

使用 OpenAPI/Swagger 保持文件最新:

```bash
# 後端產生 OpenAPI 文件
dotnet swag tofile --output contracts/openapi.yaml bin/Release/net8.0/api.dll v1

# 前端生成 TypeScript 型別
openapi-generator-cli generate -i contracts/openapi.yaml \
  -g typescript-axios -o src/api/generated
```

### 協調會議

- **每日站會 (10 分鐘)**: 同步進度、討論阻礙
- **雙週計劃會**: 計劃下期迭代
- **測試協調會**: 整合測試前協調

### 常見協調問題

| 問題 | 解決方案 |
|------|---------|
| API 延遲交付 | 前端先使用 Mock API，後端完成後再整合 |
| 型別不匹配 | 使用 OpenAPI 規格作為真理來源 |
| 環境差異 | 使用 Docker Compose 統一開發環境 |
| 資料格式變更 | 版本化 API (`/api/v1/`, `/api/v2/`) |

---

## Mock API 開發 (前端獨立開發)

若後端 API 尚未完成，前端可使用 MSW (Mock Service Worker) 模擬:

`src/mocks/handlers.ts`:

```typescript
import { http, HttpResponse } from 'msw';

export const handlers = [
  http.post('/api/auth/login', () => {
    return HttpResponse.json({
      token: 'mock-jwt-token',
      user: {
        id: '1',
        adAccount: 'user@company.com',
        name: '王小明',
        role: 'applicant',
      },
    });
  }),

  http.get('/api/appointments', () => {
    return HttpResponse.json({
      data: [
        {
          id: '1',
          objectName: '合約 A',
          status: 'pending',
          timeStart: '2025-12-20T09:00:00Z',
        },
      ],
      total: 1,
      page: 1,
    });
  }),
];
```

`src/mocks/server.ts`:

```typescript
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

export const server = setupServer(...handlers);
```

在測試或開發環境中啟用:

```typescript
if (process.env.NODE_ENV === 'development' || process.env.VITE_MOCK_API) {
  import('./mocks/server').then(({ server }) => {
    server.listen({ onUnhandledRequest: 'error' });
  });
}
```

---

## 結論

此整合指南確保前端和後端能夠有效協調。通過明確的 API 契約、清晰的認證流程、以及良好的錯誤處理，雙方團隊可以獨立開發，最後無縫整合。
