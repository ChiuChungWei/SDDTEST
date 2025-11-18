# 契約審查預約系統 - 前端技術研究

**日期**: 2025-11-18  
**狀態**: 完成  
**技術棧**: React 19.2.0 + TypeScript 5.6 + Bootstrap  

---

## 前端技術決策

### 1. React 19.2.0 + TypeScript 5.6

**決策**: 使用 React 19.2.0 與 TypeScript 5.6 進行前端開發

**理由**:
- React 19.2.0 提供最新的 Hook 和 Concurrent Features
- TypeScript 5.6 提供完整的類型安全和開發體驗
- 與後端 ASP.NET Core 分離架構相容

**替代方案考量**:
- Vue 3: 較輕量級，但團隊若熟悉 React 則效率更高
- Angular: 功能完整但學習曲線陡峭
- Svelte: 新興框架，生態系統不如 React 成熟

**實施**:
```bash
npm create vite@latest contract-review-scheduler -- --template react-ts
```

---

### 2. Bootstrap 5.3+ 作為 UI 元件庫

**決策**: 使用 Bootstrap 進行樣式和 UI 元件

**理由**:
- Bootstrap 提供完整的元件庫（表單、模態框、導航等）
- 支援 Responsive Design（RWD）
- 可客製化主題顏色和樣式
- 與 React 整合簡單（react-bootstrap）

**替代方案考量**:
- Tailwind CSS: 實用優先但需要編寫更多自訂程式碼
- Material-UI: 完整但體積較大
- Ant Design: 企業級但不適合簡單應用

**實施**:
```bash
npm install bootstrap react-bootstrap
npm install sass
```

**客製化主題**:
```scss
// src/styles/variables.scss
$primary: #007bff;
$secondary: #6c757d;
$danger: #dc3545;

@import "~bootstrap/scss/bootstrap";
```

---

### 3. Axios 作為 HTTP 客戶端

**決策**: 使用 Axios 進行 REST API 通訊

**理由**:
- 簡潔的 API 設計
- 自動 JSON 轉換
- 請求/回應攔截器支援
- 錯誤處理完整

**替代方案考量**:
- Fetch API: 原生但需要更多程式碼
- React Query: 資料獲取層但可搭配 Axios

**實施**:
```bash
npm install axios
```

**配置**:
```typescript
// src/api/client.ts
import axios from 'axios';

const client = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// 請求攔截器 - 新增 JWT Token
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default client;
```

---

### 4. React Context API + useReducer 作為狀態管理

**決策**: 使用 Context API + useReducer，避免 Redux 複雜性

**理由**:
- 無需額外依賴（Redux 開銷大）
- 足夠簡單應用使用
- TypeScript 支援完整
- 可應對預約系統的複雜狀態

**替代方案考量**:
- Redux: 功能強大但複雜度高，不適合此規模
- Zustand: 輕量但 Context API 已足夠
- MobX: 資料綁定型但學習曲線陡峭

**狀態結構**:
```typescript
// src/contexts/AppointmentContext.tsx
interface AppointmentState {
  appointments: Appointment[];
  currentAppointment: Appointment | null;
  loading: boolean;
  error: string | null;
  selectedReviewer: User | null;
  selectedDate: Date | null;
}

type AppointmentAction =
  | { type: 'SET_APPOINTMENTS'; payload: Appointment[] }
  | { type: 'SET_CURRENT'; payload: Appointment }
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'SELECT_REVIEWER'; payload: User }
  | { type: 'SELECT_DATE'; payload: Date };
```

---

### 5. React Big Calendar 作為月曆元件

**決策**: 使用 react-big-calendar 進行月曆展示

**理由**:
- 支援月、周、日視圖
- 拖拽事件支援
- 時段自訂義
- 與 React 深度整合

**替代方案考量**:
- FullCalendar: 功能更豐富但更複雜
- React Calendar: 簡單但功能較少
- 自訂實現: 開發成本高

**實施**:
```bash
npm install react-big-calendar
npm install --save-dev @types/react-big-calendar
```

**使用**:
```typescript
import { Calendar, momentLocalizer } from 'react-big-calendar';
import moment from 'moment';

const localizer = momentLocalizer(moment);

<Calendar
  localizer={localizer}
  events={appointments}
  startAccessor="date"
  endAccessor="timeEnd"
  style={{ height: 600 }}
  onSelectEvent={handleSelectAppointment}
/>
```

---

### 6. React Router 進行客戶端路由

**決策**: 使用 React Router v6 進行頁面導航

**理由**:
- 標準化的路由解決方案
- 支援嵌套路由和懶加載
- TypeScript 類型完整
- 與 React 19 相容

**路由結構**:
```typescript
// src/routes.tsx
export const routes = [
  {
    path: '/',
    element: <Layout />,
    children: [
      { path: '', element: <Dashboard /> },
      { path: 'appointments', element: <AppointmentList /> },
      { path: 'appointments/create', element: <AppointmentForm /> },
      { path: 'appointments/:id', element: <AppointmentDetail /> },
      { path: 'calendar', element: <Calendar /> },
      { path: 'admin/leave', element: <LeaveSchedule /> },
    ],
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
];
```

---

### 7. JWT Token 認證（本機儲存）

**決策**: 使用 JWT Token 存放於 localStorage，實現前端認證

**理由**:
- 無狀態認證機制
- 與後端 REST API 相容
- 簡單實現認證狀態

**替代方案考量**:
- Session Cookies: 需要伺服器支援
- OAuth2: 複雜度高，適合第三方認證

**實施**:
```typescript
// src/contexts/AuthContext.tsx
interface AuthContextType {
  isAuthenticated: boolean;
  user: User | null;
  login: (adAccount: string, password: string) => Promise<void>;
  logout: () => void;
  getToken: () => string | null;
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

**Token 管理**:
```typescript
const login = async (adAccount: string, password: string) => {
  const response = await client.post('/auth/login', { adAccount, password });
  const { token, user } = response.data;
  localStorage.setItem('authToken', token);
  setUser(user);
};

const logout = () => {
  localStorage.removeItem('authToken');
  setUser(null);
};
```

---

### 8. Formik + Yup 進行表單驗證

**決策**: 使用 Formik 管理表單狀態，Yup 進行驗證

**理由**:
- Formik 簡化複雜表單狀態管理
- Yup 提供聲明式驗證規則
- TypeScript 類型完整
- 與 React 深度整合

**替代方案考量**:
- React Hook Form: 輕量級但功能較少
- Final Form: 功能完整但用戶較少
- 手動驗證: 開發成本高

**使用**:
```typescript
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';

const validationSchema = Yup.object({
  objectName: Yup.string()
    .min(1, '契約物件名稱必填')
    .max(500, '契約物件名稱不超過 500 字')
    .required('必填'),
  timeStart: Yup.string()
    .required('開始時間必填')
    .matches(/^\d{2}:\d{2}$/, '時間格式為 HH:MM'),
  timeEnd: Yup.string()
    .required('結束時間必填')
    .test('isAfterStart', '結束時間必須晚於開始時間', function (value) {
      const { timeStart } = this.parent;
      return timeStart && value && value > timeStart;
    }),
});
```

---

### 9. ESLint + Prettier 進行程式碼品質

**決策**: 使用 ESLint 進行程式碼檢查，Prettier 進行格式化

**理由**:
- ESLint 捕捉常見程式碼問題
- Prettier 保持一致的程式碼風格
- 與 TypeScript 完整整合
- 提高團隊協作效率

**配置**:
```json
// .eslintrc.json
{
  "extends": [
    "eslint:recommended",
    "plugin:react/recommended",
    "plugin:@typescript-eslint/recommended",
    "plugin:prettier/recommended"
  ],
  "rules": {
    "react/react-in-jsx-scope": "off"
  }
}
```

---

### 10. Vitest + React Testing Library 進行測試

**決策**: 使用 Vitest 進行單元測試，React Testing Library 進行元件測試

**理由**:
- Vitest 速度快（基於 Vite）
- React Testing Library 強制最佳實踐
- TypeScript 類型完整
- 與 React 19 相容

**測試覆蓋率目標**: 80%+

**實施**:
```bash
npm install --save-dev vitest @vitest/ui react-testing-library jsdom
```

**測試範例**:
```typescript
// src/__tests__/components/AppointmentForm.test.tsx
import { render, screen, fireEvent } from '@testing-library/react';
import AppointmentForm from '../../components/AppointmentForm';

describe('AppointmentForm', () => {
  it('should submit form with valid data', async () => {
    const handleSubmit = vi.fn();
    render(<AppointmentForm onSubmit={handleSubmit} />);
    
    // 填寫表單
    fireEvent.change(screen.getByLabelText(/契約物件名稱/i), {
      target: { value: 'Contract A' },
    });
    
    // 提交表單
    fireEvent.click(screen.getByRole('button', { name: /提交/i }));
    
    // 驗證
    expect(handleSubmit).toHaveBeenCalled();
  });
});
```

---

### 11. i18n-next 進行國際化（可選）

**決策**: 預留國際化架構，目前專注繁體中文

**理由**:
- 為未來多語言支援預留空間
- 集中管理文字資源
- 易於維護翻譯內容

**實施**:
```bash
npm install i18next react-i18next
```

**配置**:
```typescript
// src/i18n/config.ts
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import zhTW from './locales/zh-TW.json';

i18n.use(initReactI18next).init({
  resources: {
    'zh-TW': { translation: zhTW },
  },
  lng: 'zh-TW',
  interpolation: { escapeValue: false },
});
```

---

## 前端架構決策

### 資料夾結構

```
frontend/
├── src/
│   ├── components/           # React 元件
│   │   ├── Layout/
│   │   ├── Calendar/
│   │   ├── AppointmentForm/
│   │   ├── AppointmentList/
│   │   ├── LeaveSchedule/
│   │   └── Auth/
│   ├── pages/               # 頁面組件
│   │   ├── Dashboard.tsx
│   │   ├── AppointmentPage.tsx
│   │   ├── LoginPage.tsx
│   │   └── AdminPage.tsx
│   ├── contexts/            # Context 和狀態
│   │   ├── AuthContext.tsx
│   │   ├── AppointmentContext.tsx
│   │   └── NotificationContext.tsx
│   ├── hooks/               # 自訂 Hooks
│   │   ├── useAuth.ts
│   │   ├── useAppointments.ts
│   │   └── useCalendar.ts
│   ├── api/                 # API 客戶端
│   │   ├── client.ts
│   │   ├── appointmentApi.ts
│   │   ├── authApi.ts
│   │   └── leaveApi.ts
│   ├── types/               # TypeScript 類型定義
│   │   ├── appointment.ts
│   │   ├── user.ts
│   │   └── common.ts
│   ├── styles/              # 全域樣式
│   │   ├── variables.scss
│   │   ├── globals.scss
│   │   └── theme.scss
│   ├── utils/               # 工具函數
│   │   ├── dateUtils.ts
│   │   ├── validation.ts
│   │   └── formatters.ts
│   ├── __tests__/           # 測試檔案
│   │   ├── components/
│   │   ├── hooks/
│   │   └── utils/
│   ├── i18n/                # 國際化
│   │   ├── config.ts
│   │   └── locales/
│   ├── App.tsx              # 主應用組件
│   ├── main.tsx             # 應用入口
│   └── vite-env.d.ts        # Vite 環境宣告
├── public/                  # 靜態資源
├── index.html               # HTML 入口
├── package.json
├── tsconfig.json
├── vite.config.ts           # Vite 配置
├── vitest.config.ts         # Vitest 配置
└── .eslintrc.json          # ESLint 配置
```

---

## 前端與後端整合點

### API 端點消費

前端將消費以下後端 API 端點：

```typescript
// 認證
POST   /api/auth/login              → 登入
GET    /api/auth/profile            → 取得使用者資訊

// 預約
POST   /api/appointments            → 建立預約
GET    /api/appointments            → 列表查詢
GET    /api/appointments/:id        → 查詢詳情
PUT    /api/appointments/:id        → 更新預約
DELETE /api/appointments/:id        → 取消預約
POST   /api/appointments/:id/accept → 接受預約
POST   /api/appointments/:id/reject → 拒絕預約

// 轉送
POST   /api/appointments/:id/delegate           → 轉送
POST   /api/appointments/:id/accept-delegation  → 代理接受
POST   /api/appointments/:id/reject-delegation  → 代理拒絕

// 月曆
GET    /api/calendar/:reviewerId/month → 月曆視圖

// 休假
POST   /api/leave-schedules         → 建立休假
GET    /api/leave-schedules         → 查詢休假
DELETE /api/leave-schedules/:id     → 刪除休假
```

---

## 效能最佳化

### 代碼分割（Code Splitting）
```typescript
// 使用 React.lazy 進行懶加載
const Dashboard = React.lazy(() => import('./pages/Dashboard'));
const AdminPage = React.lazy(() => import('./pages/AdminPage'));
```

### 請求最佳化
- 前端應使用 Axios 攔截器避免重複登入
- 實現請求取消機制（AbortController）
- 緩存月曆資料（每 5 分鐘重新整理）

### 組件最佳化
- 使用 React.memo 避免不必要的重新渲染
- 使用 useCallback 最佳化事件處理
- 使用 useMemo 最佳化計算

---

## 安全考量

### XSS 防護
- 使用 DOMPurify 清理用戶輸入（若允許 HTML）
- React 預設進行 HTML 轉義

### CSRF 防護
- 使用 JWT Token（無狀態）
- 後端驗證 Origin 和 Referer

### 敏感資訊保護
- JWT Token 存放於 localStorage（無 HttpOnly Cookie）
- 不在前端儲存敏感資訊（密碼、個人信息）
- HTTPS 傳輸加密

---

## 開發工作流程

### 本機開發
```bash
npm install
npm run dev       # 啟動開發伺服器 (http://localhost:5173)
npm run test      # 執行測試
npm run lint      # 執行 ESLint
npm run format    # 執行 Prettier 格式化
npm run build     # 建立生產版本
```

### 環境變數
```env
# .env
REACT_APP_API_URL=http://localhost:5000/api

# .env.production
REACT_APP_API_URL=https://api.company.com/api
```

---

## 程式碼品質要求

遵循專案憲章要求：
- ✅ 最小程式碼涵蓋率: 80%
- ✅ 最大循環複雜度: 10
- ✅ 所有公開元件有 JSDoc 註解
- ✅ 使用 TypeScript 進行類型安全
- ✅ 遵循 SOLID 原則
- ✅ 所有文字使用繁體中文 (zh-TW)

---

**狀態**: ✅ 完成  
**下一步**: 進行前端 UI/UX 設計和元件規劃
