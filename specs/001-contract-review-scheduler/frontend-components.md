# 契約審查預約系統 - 前端元件與狀態設計

**日期**: 2025-11-18  
**狀態**: 完成  
**技術**: React 19.2.0 + TypeScript 5.6 + Bootstrap  

---

## 前端核心元件結構

### 1. Layout 元件組 (共享)

#### App Shell
```typescript
// src/components/Layout/AppShell.tsx
interface AppShellProps {
  children: React.ReactNode;
}

export const AppShell: React.FC<AppShellProps> = ({ children }) => (
  <div className="d-flex flex-column min-vh-100">
    <Navigation />
    <main className="flex-grow-1">{children}</main>
    <Footer />
  </div>
);
```

#### Navigation 導航
```typescript
// src/components/Layout/Navigation.tsx
export const Navigation: React.FC = () => {
  const { user, logout } = useAuth();
  
  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-primary">
      <div className="container-fluid">
        <Link className="navbar-brand" to="/">
          契約審查預約系統
        </Link>
        <div className="navbar-nav ms-auto">
          {user?.role === 'applicant' && (
            <>
              <Link className="nav-link" to="/appointments/create">
                建立預約
              </Link>
              <Link className="nav-link" to="/appointments">
                我的預約
              </Link>
            </>
          )}
          {user?.role === 'reviewer' && (
            <>
              <Link className="nav-link" to="/calendar">
                月曆
              </Link>
              <Link className="nav-link" to="/admin/leave">
                設定休假
              </Link>
            </>
          )}
          <button className="btn btn-outline-light ms-2" onClick={logout}>
            登出
          </button>
        </div>
      </div>
    </nav>
  );
};
```

---

### 2. 認證相關元件

#### LoginPage 登入頁面
```typescript
// src/pages/LoginPage.tsx
interface LoginFormData {
  adAccount: string;
  password: string;
}

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (values: LoginFormData) => {
    try {
      await login(values.adAccount, values.password);
      navigate('/');
    } catch (error) {
      // 錯誤處理
    }
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-body">
              <h2 className="card-title text-center mb-4">登入系統</h2>
              <LoginForm onSubmit={handleSubmit} />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
```

#### LoginForm 登入表單
```typescript
// src/components/Auth/LoginForm.tsx
interface LoginFormProps {
  onSubmit: (data: { adAccount: string; password: string }) => Promise<void>;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onSubmit }) => {
  const formik = useFormik({
    initialValues: {
      adAccount: '',
      password: '',
    },
    validationSchema: Yup.object({
      adAccount: Yup.string()
        .min(3, 'AD 帳號至少 3 個字')
        .required('必填'),
      password: Yup.string()
        .min(6, '密碼至少 6 個字')
        .required('必填'),
    }),
    onSubmit,
  });

  return (
    <form onSubmit={formik.handleSubmit}>
      <div className="mb-3">
        <label htmlFor="adAccount" className="form-label">
          AD 帳號
        </label>
        <input
          id="adAccount"
          type="text"
          className={`form-control ${
            formik.touched.adAccount && formik.errors.adAccount
              ? 'is-invalid'
              : ''
          }`}
          {...formik.getFieldProps('adAccount')}
        />
        {formik.touched.adAccount && formik.errors.adAccount && (
          <div className="invalid-feedback">{formik.errors.adAccount}</div>
        )}
      </div>
      <div className="mb-3">
        <label htmlFor="password" className="form-label">
          密碼
        </label>
        <input
          id="password"
          type="password"
          className={`form-control ${
            formik.touched.password && formik.errors.password
              ? 'is-invalid'
              : ''
          }`}
          {...formik.getFieldProps('password')}
        />
      </div>
      <button type="submit" className="btn btn-primary w-100">
        登入
      </button>
    </form>
  );
};
```

---

### 3. 預約相關元件

#### AppointmentForm 預約表單
```typescript
// src/components/Appointment/AppointmentForm.tsx
interface AppointmentFormProps {
  onSubmit: (data: CreateAppointmentRequest) => Promise<void>;
  initialValues?: Appointment;
}

export const AppointmentForm: React.FC<AppointmentFormProps> = ({
  onSubmit,
  initialValues,
}) => {
  const { data: reviewers } = useReviewers();

  const validationSchema = Yup.object({
    objectName: Yup.string()
      .min(1, '必填')
      .max(500, '不超過 500 字')
      .required(),
    reviewerId: Yup.string().required('必選'),
    date: Yup.date().required('必填'),
    timeStart: Yup.string()
      .matches(/^\d{2}:\d{2}$/, '格式為 HH:MM')
      .required(),
    timeEnd: Yup.string()
      .matches(/^\d{2}:\d{2}$/, '格式為 HH:MM')
      .required(),
  });

  const formik = useFormik({
    initialValues: initialValues || {
      objectName: '',
      reviewerId: '',
      date: new Date(),
      timeStart: '09:00',
      timeEnd: '09:15',
    },
    validationSchema,
    onSubmit,
  });

  return (
    <form onSubmit={formik.handleSubmit}>
      <div className="mb-3">
        <label htmlFor="objectName" className="form-label">
          契約物件名稱
        </label>
        <input
          id="objectName"
          type="text"
          className="form-control"
          {...formik.getFieldProps('objectName')}
        />
      </div>

      <div className="mb-3">
        <label htmlFor="reviewerId" className="form-label">
          選擇審查人員
        </label>
        <select
          id="reviewerId"
          className="form-select"
          {...formik.getFieldProps('reviewerId')}
        >
          <option value="">-- 選擇 --</option>
          {reviewers?.map((reviewer) => (
            <option key={reviewer.id} value={reviewer.id}>
              {reviewer.name}
            </option>
          ))}
        </select>
      </div>

      <div className="row">
        <div className="col-md-6 mb-3">
          <label htmlFor="date" className="form-label">
            預約日期
          </label>
          <input
            id="date"
            type="date"
            className="form-control"
            {...formik.getFieldProps('date')}
          />
        </div>
        <div className="col-md-3 mb-3">
          <label htmlFor="timeStart" className="form-label">
            開始時間
          </label>
          <input
            id="timeStart"
            type="time"
            className="form-control"
            {...formik.getFieldProps('timeStart')}
          />
        </div>
        <div className="col-md-3 mb-3">
          <label htmlFor="timeEnd" className="form-label">
            結束時間
          </label>
          <input
            id="timeEnd"
            type="time"
            className="form-control"
            {...formik.getFieldProps('timeEnd')}
          />
        </div>
      </div>

      <button type="submit" className="btn btn-primary">
        提交預約
      </button>
    </form>
  );
};
```

#### AppointmentCard 預約卡片
```typescript
// src/components/Appointment/AppointmentCard.tsx
interface AppointmentCardProps {
  appointment: Appointment;
  onAction?: (action: 'accept' | 'reject' | 'edit' | 'cancel') => void;
}

export const AppointmentCard: React.FC<AppointmentCardProps> = ({
  appointment,
  onAction,
}) => {
  const getStatusBadge = (status: string) => {
    const statusMap: Record<string, string> = {
      pending: 'warning',
      accepted: 'success',
      rejected: 'danger',
      cancelled: 'secondary',
    };
    return statusMap[status] || 'info';
  };

  return (
    <div className="card mb-3">
      <div className="card-body">
        <h5 className="card-title">{appointment.objectName}</h5>
        <p className="card-text">
          <small className="text-muted">
            {appointment.date} {appointment.timeStart} - {appointment.timeEnd}
          </small>
        </p>
        <span className={`badge bg-${getStatusBadge(appointment.status)}`}>
          {appointment.status}
        </span>
        {onAction && (
          <div className="mt-3">
            <button
              className="btn btn-sm btn-success me-2"
              onClick={() => onAction('accept')}
            >
              接受
            </button>
            <button
              className="btn btn-sm btn-danger"
              onClick={() => onAction('reject')}
            >
              拒絕
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
```

#### AppointmentList 預約列表
```typescript
// src/pages/AppointmentPage.tsx
export const AppointmentPage: React.FC = () => {
  const { appointments, loading, error } = useAppointments();

  if (loading) return <div className="spinner-border"></div>;
  if (error) return <div className="alert alert-danger">{error}</div>;

  return (
    <div className="container mt-4">
      <h2>我的預約</h2>
      <div className="row">
        {appointments?.map((apt) => (
          <div key={apt.id} className="col-md-6">
            <AppointmentCard
              appointment={apt}
              onAction={(action) => handleAppointmentAction(apt.id, action)}
            />
          </div>
        ))}
      </div>
    </div>
  );
};
```

---

### 4. 月曆相關元件

#### CalendarView 月曆視圖
```typescript
// src/components/Calendar/CalendarView.tsx
import { Calendar, momentLocalizer } from 'react-big-calendar';
import moment from 'moment';

const localizer = momentLocalizer(moment);

interface CalendarViewProps {
  events: Appointment[];
  onSelectEvent?: (appointment: Appointment) => void;
  onSelectSlot?: (slotInfo: any) => void;
}

export const CalendarView: React.FC<CalendarViewProps> = ({
  events,
  onSelectEvent,
  onSelectSlot,
}) => {
  return (
    <div style={{ height: 600 }}>
      <Calendar
        localizer={localizer}
        events={events}
        startAccessor="dateStart"
        endAccessor="dateEnd"
        selectable
        onSelectEvent={onSelectEvent}
        onSelectSlot={onSelectSlot}
        views={['month', 'week', 'day']}
        defaultView="month"
      />
    </div>
  );
};
```

---

### 5. 休假管理元件

#### LeaveScheduleForm 休假表單
```typescript
// src/components/LeaveSchedule/LeaveScheduleForm.tsx
interface LeaveScheduleFormProps {
  onSubmit: (data: CreateLeaveScheduleRequest) => Promise<void>;
}

export const LeaveScheduleForm: React.FC<LeaveScheduleFormProps> = ({
  onSubmit,
}) => {
  const formik = useFormik({
    initialValues: {
      date: new Date(),
      timeStart: '09:00',
      timeEnd: '18:00',
      reason: '',
    },
    validationSchema: Yup.object({
      date: Yup.date().required('必填'),
      timeStart: Yup.string().required('必填'),
      timeEnd: Yup.string().required('必填'),
    }),
    onSubmit,
  });

  return (
    <form onSubmit={formik.handleSubmit}>
      <div className="mb-3">
        <label htmlFor="date" className="form-label">
          休假日期
        </label>
        <input
          id="date"
          type="date"
          className="form-control"
          {...formik.getFieldProps('date')}
        />
      </div>

      <div className="row">
        <div className="col-md-6 mb-3">
          <label htmlFor="timeStart" className="form-label">
            開始時間
          </label>
          <input
            id="timeStart"
            type="time"
            className="form-control"
            {...formik.getFieldProps('timeStart')}
          />
        </div>
        <div className="col-md-6 mb-3">
          <label htmlFor="timeEnd" className="form-label">
            結束時間
          </label>
          <input
            id="timeEnd"
            type="time"
            className="form-control"
            {...formik.getFieldProps('timeEnd')}
          />
        </div>
      </div>

      <div className="mb-3">
        <label htmlFor="reason" className="form-label">
          休假原因 (選擇性)
        </label>
        <input
          id="reason"
          type="text"
          className="form-control"
          {...formik.getFieldProps('reason')}
        />
      </div>

      <button type="submit" className="btn btn-primary">
        新增休假
      </button>
    </form>
  );
};
```

---

## TypeScript 類型定義

```typescript
// src/types/index.ts

// 使用者類型
export interface User {
  id: string;
  adAccount: string;
  name: string;
  email: string;
  role: 'applicant' | 'reviewer';
  isActive: boolean;
  lastLoginAt?: Date;
}

// 預約類型
export interface Appointment {
  id: string;
  applicantId: string;
  reviewerId: string;
  date: string; // YYYY-MM-DD
  dateStart: Date; // 用於月曆
  dateEnd: Date; // 用於月曆
  timeStart: string; // HH:MM
  timeEnd: string; // HH:MM
  objectName: string;
  status: 'pending' | 'accepted' | 'rejected' | 'delegated' | 'cancelled';
  delegateReviewerId?: string;
  delegateStatus?: 'pending' | 'accepted' | 'rejected';
  createdAt: Date;
  updatedAt: Date;
}

// 建立預約請求
export interface CreateAppointmentRequest {
  reviewerId: string;
  date: Date;
  timeStart: string;
  timeEnd: string;
  objectName: string;
}

// 休假排程類型
export interface LeaveSchedule {
  id: string;
  reviewerId: string;
  date: string;
  timeStart: string;
  timeEnd: string;
  reason?: string;
  createdAt: Date;
}

// API 回應包裝
export interface ApiResponse<T> {
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: Record<string, string>;
  };
  timestamp: string;
}
```

---

## 前端狀態管理

### AuthContext
```typescript
// src/contexts/AuthContext.tsx
interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: (adAccount: string, password: string) => Promise<void>;
  logout: () => void;
  getToken: () => string | null;
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const login = async (adAccount: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await authApi.login(adAccount, password);
      localStorage.setItem('authToken', response.token);
      setUser(response.user);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, error, login, logout, getToken }}>
      {children}
    </AuthContext.Provider>
  );
};
```

---

## 自訂 Hooks

### useAppointments
```typescript
// src/hooks/useAppointments.ts
export const useAppointments = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchAppointments = async () => {
    setLoading(true);
    try {
      const data = await appointmentApi.getAppointments();
      setAppointments(data);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const createAppointment = async (data: CreateAppointmentRequest) => {
    return await appointmentApi.createAppointment(data);
  };

  useEffect(() => {
    fetchAppointments();
  }, []);

  return { appointments, loading, error, createAppointment, refetch: fetchAppointments };
};
```

---

## 頁面路由結構

```typescript
// src/routes.tsx
export const routes = [
  {
    path: '/',
    element: <AppShell />,
    children: [
      { path: '', element: <Dashboard /> },
      { path: 'appointments', element: <AppointmentListPage /> },
      { path: 'appointments/create', element: <AppointmentCreatePage /> },
      { path: 'appointments/:id', element: <AppointmentDetailPage /> },
      { path: 'calendar', element: <CalendarPage /> },
      { path: 'admin/leave', element: <LeaveSchedulePage /> },
    ],
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '*',
    element: <NotFoundPage />,
  },
];
```

---

## 最佳實踐

### 元件命名
- 使用 PascalCase 命名元件檔案和類別
- 檔案名稱應與元件名稱相同
- 例：`AppointmentForm.tsx` 內匯出 `AppointmentForm` 元件

### 資料流
- Props 由上而下傳遞
- 狀態提升到最近的公共祖先
- 使用 Context 進行全域狀態（Auth, Notifications）

### 錯誤處理
- 使用 try-catch 捕捉 API 錯誤
- 顯示使用者友善的錯誤訊息
- 記錄完整的錯誤堆棧用於除錯

---

**狀態**: ✅ 完成  
**下一步**: 進行前端任務分解和開發計劃
