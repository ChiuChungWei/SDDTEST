# 契約審查預約系統 - 前端開發指南

**日期**: 2025-11-18  
**版本**: 1.0  
**技術**: React 19.2.0 + TypeScript 5.6 + Bootstrap  

---

## 前置需求

### 開發環境

| 工具 | 版本 | 用途 |
|------|------|------|
| Node.js | 18+ | JavaScript 執行時 |
| npm | 10+ | 套件管理 |
| VS Code | Latest | 程式碼編輯器 |
| Git | 2.0+ | 版本控制 |

### 系統需求
- Windows 10+, macOS 11+, 或 Ubuntu 20.04+
- 4GB RAM 最小
- 2GB 磁碟空間

---

## 快速開始

### 1. 安裝 Node.js 和 npm

#### Windows
```powershell
# 使用 Chocolatey
choco install nodejs

# 或從 https://nodejs.org 下載安裝程式
```

#### macOS
```bash
brew install node
```

#### Linux (Ubuntu)
```bash
sudo apt-get update
sudo apt-get install nodejs npm
```

**驗證安裝**:
```bash
node --version  # 應顯示 v18.x.x 或更高
npm --version   # 應顯示 10.x.x 或更高
```

---

### 2. 建立新專案

#### 使用 Vite 建立 React 專案
```bash
npm create vite@latest contract-review-frontend -- --template react-ts
cd contract-review-frontend
npm install
```

#### 或使用 Create React App (較慢但更簡單)
```bash
npx create-react-app contract-review-frontend --template typescript
cd contract-review-frontend
```

**建議使用 Vite**（更快的開發體驗）

---

### 3. 安裝依賴套件

```bash
npm install \
  react-bootstrap bootstrap \
  axios \
  react-router-dom \
  formik yup \
  react-big-calendar moment \
  react-i18next i18next

# 開發依賴
npm install --save-dev \
  eslint prettier \
  vitest @vitest/ui react-testing-library jsdom \
  @typescript-eslint/eslint-plugin \
  @typescript-eslint/parser
```

或編輯 `package.json` 後執行 `npm install`:

```json
{
  "dependencies": {
    "react": "^19.2.0",
    "react-dom": "^19.2.0",
    "react-bootstrap": "^2.10.0",
    "bootstrap": "^5.3.0",
    "axios": "^1.6.0",
    "react-router-dom": "^6.20.0",
    "formik": "^2.4.0",
    "yup": "^1.3.0",
    "react-big-calendar": "^1.8.0",
    "moment": "^2.29.0",
    "react-i18next": "^13.5.0",
    "i18next": "^23.7.0"
  },
  "devDependencies": {
    "@vitejs/plugin-react": "^4.0.0",
    "vite": "^5.0.0",
    "typescript": "^5.6.0",
    "@types/react": "^19.0.0",
    "@types/react-dom": "^19.0.0",
    "@types/react-big-calendar": "^1.8.0",
    "vitest": "^1.0.0",
    "@vitest/ui": "^1.0.0",
    "react-testing-library": "^14.0.0",
    "jsdom": "^23.0.0",
    "eslint": "^8.50.0",
    "prettier": "^3.0.0"
  }
}
```

---

### 4. 專案結構設置

```bash
mkdir -p src/{components,pages,contexts,hooks,api,types,styles,utils,__tests__}
```

**完整結構**:
```
frontend/
├── src/
│   ├── components/         # React 元件
│   ├── pages/              # 頁面組件
│   ├── contexts/           # Context API
│   ├── hooks/              # 自訂 Hooks
│   ├── api/                # API 客戶端
│   ├── types/              # TypeScript 類型
│   ├── styles/             # CSS / SCSS
│   ├── utils/              # 工具函數
│   ├── __tests__/          # 測試檔案
│   ├── App.tsx
│   ├── main.tsx
│   └── vite-env.d.ts
├── public/
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
├── vitest.config.ts
├── .eslintrc.json
├── .prettierrc
└── .gitignore
```

---

### 5. 配置 Vite

建立 `vite.config.ts`:

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, '/api'),
      }
    }
  },
  resolve: {
    alias: {
      '@': '/src',
    }
  }
})
```

---

### 6. 配置環境變數

建立 `.env`:
```
VITE_API_URL=http://localhost:5000/api
```

建立 `.env.production`:
```
VITE_API_URL=https://api.company.com/api
```

在程式碼中使用:
```typescript
const apiUrl = import.meta.env.VITE_API_URL;
```

---

## 常用命令

```bash
# 開發伺服器 (http://localhost:5173)
npm run dev

# 生產構建
npm run build

# 預覽構建結果
npm run preview

# 執行測試
npm run test

# 監視模式測試
npm run test:watch

# 測試覆蓋率報告
npm run test:coverage

# ESLint 檢查
npm run lint

# Prettier 格式化
npm run format

# 建置並執行預覽
npm run build && npm run preview
```

---

## 開發工作流程

### 1. 建立元件

新建檔案 `src/components/MyComponent.tsx`:

```typescript
import React from 'react';

interface MyComponentProps {
  title: string;
  onSubmit?: (data: any) => void;
}

export const MyComponent: React.FC<MyComponentProps> = ({
  title,
  onSubmit,
}) => {
  return (
    <div className="my-component">
      <h2>{title}</h2>
    </div>
  );
};

export default MyComponent;
```

### 2. 在頁面中使用

`src/pages/MyPage.tsx`:

```typescript
import { MyComponent } from '../components/MyComponent';

export const MyPage: React.FC = () => {
  return (
    <div className="container mt-4">
      <MyComponent title="我的元件" />
    </div>
  );
};
```

### 3. 在路由中註冊

`src/App.tsx`:

```typescript
import { Routes, Route } from 'react-router-dom';
import { MyPage } from './pages/MyPage';

function App() {
  return (
    <Routes>
      <Route path="/my-page" element={<MyPage />} />
    </Routes>
  );
}
```

---

## Bootstrap 主題客製化

建立 `src/styles/variables.scss`:

```scss
// Bootstrap 變數覆蓋
$primary: #007bff;
$secondary: #6c757d;
$success: #28a745;
$danger: #dc3545;
$warning: #ffc107;
$info: #17a2b8;
$light: #f8f9fa;
$dark: #343a40;

// 匯入 Bootstrap
@import '~bootstrap/scss/bootstrap';

// 自訂樣式
body {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}
```

在 `src/main.tsx` 中匯入:

```typescript
import './styles/variables.scss';
```

---

## 認證設置

### 建立 AuthContext

`src/contexts/AuthContext.tsx`:

```typescript
import { createContext, useContext, useState } from 'react';
import axiosClient from '../api/client';

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
  login: (adAccount: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);

  const login = async (adAccount: string, password: string) => {
    const response = await axiosClient.post('/auth/login', {
      adAccount,
      password,
    });
    const { token, user } = response.data;
    localStorage.setItem('authToken', token);
    setUser(user);
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
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

## API 客戶端設置

`src/api/client.ts`:

```typescript
import axios from 'axios';

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 10000,
});

// 請求攔截器
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 回應攔截器
client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default client;
```

---

## 表單驗證 (Formik + Yup)

```typescript
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';

const validationSchema = Yup.object({
  objectName: Yup.string()
    .required('必填')
    .min(1, '至少 1 字')
    .max(500, '最多 500 字'),
  timeStart: Yup.string().required('必填'),
  timeEnd: Yup.string()
    .required('必填')
    .test('isAfter', '結束時間必須晚於開始時間', function (value) {
      const { timeStart } = this.parent;
      return !timeStart || !value || value > timeStart;
    }),
});

export const AppointmentForm = () => {
  return (
    <Formik
      initialValues={{
        objectName: '',
        timeStart: '',
        timeEnd: '',
      }}
      validationSchema={validationSchema}
      onSubmit={async (values) => {
        await client.post('/appointments', values);
      }}
    >
      {() => (
        <Form>
          <div className="mb-3">
            <label htmlFor="objectName" className="form-label">
              契約物件名稱
            </label>
            <Field
              id="objectName"
              name="objectName"
              className="form-control"
            />
            <ErrorMessage name="objectName" component="div" className="text-danger" />
          </div>
        </Form>
      )}
    </Formik>
  );
};
```

---

## 測試範例

`src/__tests__/components/LoginForm.test.tsx`:

```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { LoginForm } from '../../components/Auth/LoginForm';

describe('LoginForm', () => {
  it('should display login form', () => {
    render(<LoginForm onSubmit={vi.fn()} />);
    expect(screen.getByLabelText(/AD 帳號/i)).toBeInTheDocument();
  });

  it('should submit form with valid data', async () => {
    const handleSubmit = vi.fn();
    render(<LoginForm onSubmit={handleSubmit} />);

    fireEvent.change(screen.getByLabelText(/AD 帳號/i), {
      target: { value: 'user123' },
    });
    fireEvent.change(screen.getByLabelText(/密碼/i), {
      target: { value: 'password123' },
    });

    fireEvent.click(screen.getByRole('button', { name: /登入/i }));

    expect(handleSubmit).toHaveBeenCalled();
  });
});
```

---

## 效能最佳化

### 代碼分割

```typescript
import { lazy, Suspense } from 'react';

const Dashboard = lazy(() => import('./pages/Dashboard'));
const AdminPage = lazy(() => import('./pages/AdminPage'));

<Suspense fallback={<div>Loading...</div>}>
  <Dashboard />
</Suspense>
```

### React.memo 防止不必要重新渲染

```typescript
export const AppointmentCard = React.memo(({ appointment }: Props) => {
  return <div>{appointment.objectName}</div>;
});
```

### useMemo 和 useCallback

```typescript
const memoizedValue = useMemo(() => computeExpensiveValue(a, b), [a, b]);
const memoizedCallback = useCallback(() => {
  doSomething(a, b);
}, [a, b]);
```

---

## 調試技巧

### VS Code 調試器

建立 `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "type": "chrome",
      "request": "launch",
      "name": "Launch React",
      "url": "http://localhost:5173",
      "webRoot": "${workspaceFolder}",
      "sourceMapPathOverride": {
        "webpack:///src/*": "${webRoot}/src/*"
      }
    }
  ]
}
```

### 使用 React DevTools

安装 [React DevTools](https://chrome.google.com/webstore/detail/react-developer-tools/fmkadmapgofadopljbjfkapdkoienihi) 瀏覽器擴充

---

## 部署

### 構建生產版本

```bash
npm run build
```

產出在 `dist/` 資料夾

### 部署到 Nginx

```nginx
server {
  listen 80;
  server_name app.example.com;

  root /var/www/html/contract-review-frontend/dist;
  index index.html;

  location / {
    try_files $uri /index.html;
  }

  location /api {
    proxy_pass http://localhost:5000;
  }
}
```

### 部署到 Vercel 或 Netlify

#### Vercel
```bash
npm install -g vercel
vercel
```

#### Netlify
```bash
npm install -g netlify-cli
netlify deploy --prod --dir=dist
```

---

## 常見問題

**Q: 開發伺服器無法連接後端?**  
A: 檢查 `vite.config.ts` 中的 proxy 設定，確保後端伺服器運行在設定的 port 上。

**Q: CORS 錯誤?**  
A: 確保後端已配置 CORS middleware，允許前端域名。

**Q: 熱重載不工作?**  
A: 檢查檔案變更是否被偵測，嘗試重新啟動開發伺服器。

**Q: TypeScript 錯誤?**  
A: 執行 `npm run build` 檢查編譯錯誤，確保所有類型定義正確。

---

## 進一步閱讀

- [React 官方文檔](https://react.dev)
- [TypeScript 手冊](https://www.typescriptlang.org/docs/)
- [Bootstrap 文檔](https://getbootstrap.com/docs/)
- [React Router 文檔](https://reactrouter.com/)
- [Formik 文檔](https://formik.org)

---

**祝程式設計愉快!** 🚀
