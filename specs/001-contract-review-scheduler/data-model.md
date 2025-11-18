# 契約審查預約系統 - 資料模型

**日期**: 2025-11-18  
**狀態**: 完成

## 資料模型概覽

該系統使用以下核心實體：

```
┌─────────────────┐
│     User        │
├─────────────────┤
│ id (PK)         │
│ ad_account      │
│ name            │
│ email           │
│ role            │
└────────┬────────┘
         │
         ├─────────────────────┐
         │                     │
    ┌────▼──────────┐   ┌──────▼──────────┐
    │ Appointment   │   │ LeaveSchedule   │
    ├───────────────┤   ├─────────────────┤
    │ id (PK)       │   │ id (PK)         │
    │ applicant_id  │   │ reviewer_id (FK)│
    │ reviewer_id   │   │ date            │
    │ date          │   │ time_start      │
    │ time_start    │   │ time_end        │
    │ time_end      │   │ created_at      │
    │ object_name   │   │ updated_at      │
    │ status        │   └─────────────────┘
    │ created_at    │
    └───┬───────────┘
        │
    ┌───▼──────────────────┐
    │ AppointmentHistory   │
    ├──────────────────────┤
    │ id (PK)              │
    │ appointment_id (FK)  │
    │ action               │
    │ actor_id (FK)        │
    │ timestamp            │
    │ notes                │
    └──────────────────────┘
```

## 實體定義

### 1. User (使用者)

**用途**: 儲存系統使用者資訊

**欄位**:

| 欄位名 | 型別 | 約束 | 描述 |
|--------|------|------|------|
| id | UUID | PRIMARY KEY | 系統唯一識別碼 |
| ad_account | VARCHAR(100) | UNIQUE, NOT NULL | Active Directory 帳號 |
| name | VARCHAR(255) | NOT NULL | 使用者全名 |
| email | VARCHAR(255) | UNIQUE, NOT NULL | 電子郵件地址，格式: {ad_account}@isn.co.jp |
| role | ENUM('applicant', 'reviewer') | NOT NULL | 使用者角色：申請人或審查人員 |
| is_active | BOOLEAN | DEFAULT true | 帳號啟用狀態 |
| last_login_at | TIMESTAMP | NULL | 最後登入時間 |
| created_at | TIMESTAMP | DEFAULT NOW() | 建立時間 |
| updated_at | TIMESTAMP | DEFAULT NOW() | 更新時間 |

**驗證規則**:
- ad_account: 長度 3-100 字元，只允許字母、數字、連字號
- email: 必須符合 {ad_account}@isn.co.jp 格式
- name: 非空，長度 1-255 字元
- role: 只允許 'applicant' 或 'reviewer'

**狀態**:
- is_active = true: 使用者可登入系統
- is_active = false: 使用者無法登入（軟刪除）

**關係**:
- 1:N 與 Appointment (applicant_id, reviewer_id)
- 1:N 與 LeaveSchedule
- 1:N 與 AppointmentHistory

---

### 2. Appointment (預約)

**用途**: 儲存預約資訊

**欄位**:

| 欄位名 | 型別 | 約束 | 描述 |
|--------|------|------|------|
| id | UUID | PRIMARY KEY | 預約唯一識別碼 |
| applicant_id | UUID | FOREIGN KEY, NOT NULL | 申請人 ID |
| reviewer_id | UUID | FOREIGN KEY, NOT NULL | 審查人員 ID |
| date | DATE | NOT NULL | 預約日期 (YYYY-MM-DD) |
| time_start | TIME | NOT NULL | 預約開始時間 (HH:MM:SS) |
| time_end | TIME | NOT NULL | 預約結束時間 (HH:MM:SS) |
| object_name | VARCHAR(500) | NOT NULL | 契約物件名稱 |
| status | ENUM(...) | NOT NULL, DEFAULT 'pending' | 預約狀態 |
| delegate_reviewer_id | UUID | FOREIGN KEY, NULL | 代理審查人員 ID (若已轉送) |
| delegate_status | ENUM('pending', 'accepted', 'rejected') | NULL | 轉送狀態 |
| created_by | UUID | FOREIGN KEY, NOT NULL | 建立者 ID |
| created_at | TIMESTAMP | DEFAULT NOW() | 建立時間 |
| updated_at | TIMESTAMP | DEFAULT NOW() | 更新時間 |
| cancelled_at | TIMESTAMP | NULL | 取消時間 |
| cancelled_reason | TEXT | NULL | 取消原因 |

**預約狀態列舉**:
```
- pending: 待審查人員確認
- accepted: 審查人員已接受
- rejected: 審查人員已拒絕
- delegated: 已轉送給代理人
- delegate_accepted: 代理人已接受轉送
- delegate_rejected: 代理人已拒絕轉送
- cancelled: 已取消
```

**驗證規則**:
- date: 必須是工作日 (週一至週五)
- time_start < time_end: 開始時間必須早於結束時間
- time_start, time_end: 必須為 15 分鐘的倍數 (00:00, 00:15, 00:30, 00:45)
- time_start >= 09:00 AND time_end <= 18:00: 必須在營業時間內
- object_name: 長度 1-500 字元，非空
- applicant_id ≠ reviewer_id: 申請人和審查人員不能是同一人
- 預約時間 24 小時後才能變更/取消

**狀態轉換圖**:
```
pending → accepted → (可轉送) → delegated → delegate_accepted
       ↘ rejected
       ↘ cancelled
delegated → delegate_rejected
```

**關係**:
- N:1 與 User (applicant_id, reviewer_id, created_by, delegate_reviewer_id)
- 1:N 與 AppointmentHistory

**複合索引**:
- (reviewer_id, date, time_start, time_end)
- (applicant_id, created_at)
- (status, created_at)

---

### 3. LeaveSchedule (休假排程)

**用途**: 儲存審查人員的休假資訊

**欄位**:

| 欄位名 | 型別 | 約束 | 描述 |
|--------|------|------|------|
| id | UUID | PRIMARY KEY | 休假排程唯一識別碼 |
| reviewer_id | UUID | FOREIGN KEY, NOT NULL | 審查人員 ID |
| date | DATE | NOT NULL | 休假日期 (YYYY-MM-DD) |
| time_start | TIME | NOT NULL | 休假開始時間 (HH:MM:SS) |
| time_end | TIME | NOT NULL | 休假結束時間 (HH:MM:SS) |
| reason | VARCHAR(255) | NULL | 休假原因 |
| created_at | TIMESTAMP | DEFAULT NOW() | 建立時間 |
| updated_at | TIMESTAMP | DEFAULT NOW() | 更新時間 |

**驗證規則**:
- date: 必須是工作日 (週一至週五)
- time_start < time_end: 開始時間必須早於結束時間
- time_start, time_end: 必須為 15 分鐘的倍數
- time_start >= 09:00 AND time_end <= 18:00: 必須在營業時間內
- 同一時段內無法設定重複的休假
- 該時段內無預約才能設定休假

**關係**:
- N:1 與 User (reviewer_id)

**複合索引**:
- (reviewer_id, date, time_start, time_end)

---

### 4. AppointmentHistory (預約歷史)

**用途**: 記錄預約的所有狀態變更和操作

**欄位**:

| 欄位名 | 型別 | 約束 | 描述 |
|--------|------|------|------|
| id | UUID | PRIMARY KEY | 歷史記錄唯一識別碼 |
| appointment_id | UUID | FOREIGN KEY, NOT NULL | 預約 ID |
| action | ENUM(...) | NOT NULL | 執行的動作 |
| actor_id | UUID | FOREIGN KEY, NOT NULL | 執行者 ID |
| old_status | ENUM(...) | NULL | 變更前狀態 |
| new_status | ENUM(...) | NULL | 變更後狀態 |
| details | JSONB | NULL | 額外詳情 (JSON) |
| timestamp | TIMESTAMP | DEFAULT NOW() | 操作時間 |
| notes | TEXT | NULL | 備註 |

**動作列舉**:
```
- created: 預約已建立
- updated: 預約已更新
- accepted: 已接受
- rejected: 已拒絕
- delegated: 已轉送
- delegate_accepted: 代理人已接受
- delegate_rejected: 代理人已拒絕
- cancelled: 已取消
- reminder_sent: 提醒已發送
```

**詳情欄位範例** (JSONB):
```json
{
  "old_time_start": "09:00:00",
  "new_time_start": "10:00:00",
  "changed_fields": ["time_start"]
}
```

**關係**:
- N:1 與 Appointment (appointment_id)
- N:1 與 User (actor_id)

**複合索引**:
- (appointment_id, timestamp)
- (actor_id, timestamp)

---

### 5. NotificationLog (通知紀錄) - 可選

**用途**: 紀錄所有發送的通知，供稽核

**欄位**:

| 欄位名 | 型別 | 約束 | 描述 |
|--------|------|------|------|
| id | UUID | PRIMARY KEY | 通知紀錄唯一識別碼 |
| appointment_id | UUID | FOREIGN KEY, NOT NULL | 相關預約 ID |
| recipient_id | UUID | FOREIGN KEY, NOT NULL | 收件人 ID |
| notification_type | ENUM(...) | NOT NULL | 通知類型 |
| status | ENUM('pending', 'sent', 'failed') | NOT NULL | 發送狀態 |
| sent_at | TIMESTAMP | NULL | 發送時間 |
| retry_count | INTEGER | DEFAULT 0 | 重試次數 |
| error_message | TEXT | NULL | 錯誤訊息 |
| created_at | TIMESTAMP | DEFAULT NOW() | 建立時間 |

**通知類型列舉**:
```
- appointment_created: 新預約通知
- appointment_accepted: 預約已接受通知
- appointment_rejected: 預約已拒絕通知
- appointment_delegated: 預約已轉送通知
- delegate_accepted: 轉送已接受通知
- delegate_rejected: 轉送已拒絕通知
- appointment_cancelled: 預約已取消通知
- appointment_modified: 預約已變更通知
```

---

## 資料庫初始化指令

### 建立表格

```sql
-- Users 表
CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  ad_account VARCHAR(100) UNIQUE NOT NULL,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) UNIQUE NOT NULL,
  role ENUM('applicant', 'reviewer') NOT NULL,
  is_active BOOLEAN DEFAULT true,
  last_login_at TIMESTAMP,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- Appointments 表
CREATE TABLE appointments (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  applicant_id UUID NOT NULL REFERENCES users(id),
  reviewer_id UUID NOT NULL REFERENCES users(id),
  date DATE NOT NULL,
  time_start TIME NOT NULL,
  time_end TIME NOT NULL,
  object_name VARCHAR(500) NOT NULL,
  status VARCHAR(50) NOT NULL DEFAULT 'pending',
  delegate_reviewer_id UUID REFERENCES users(id),
  delegate_status VARCHAR(50),
  created_by UUID NOT NULL REFERENCES users(id),
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW(),
  cancelled_at TIMESTAMP,
  cancelled_reason TEXT
);

-- LeaveSchedules 表
CREATE TABLE leave_schedules (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  reviewer_id UUID NOT NULL REFERENCES users(id),
  date DATE NOT NULL,
  time_start TIME NOT NULL,
  time_end TIME NOT NULL,
  reason VARCHAR(255),
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- AppointmentHistory 表
CREATE TABLE appointment_history (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  appointment_id UUID NOT NULL REFERENCES appointments(id),
  action VARCHAR(50) NOT NULL,
  actor_id UUID NOT NULL REFERENCES users(id),
  old_status VARCHAR(50),
  new_status VARCHAR(50),
  details JSONB,
  timestamp TIMESTAMP DEFAULT NOW(),
  notes TEXT
);

-- 建立索引
CREATE INDEX idx_appointments_reviewer_date 
  ON appointments(reviewer_id, date, time_start, time_end);
CREATE INDEX idx_appointments_applicant_created 
  ON appointments(applicant_id, created_at);
CREATE INDEX idx_appointments_status_created 
  ON appointments(status, created_at);
CREATE INDEX idx_leave_schedules_reviewer_date 
  ON leave_schedules(reviewer_id, date, time_start, time_end);
CREATE INDEX idx_appointment_history_appointment_timestamp 
  ON appointment_history(appointment_id, timestamp);
CREATE INDEX idx_appointment_history_actor_timestamp 
  ON appointment_history(actor_id, timestamp);
```

---

## 資料關係說明

### User - Appointment 關係

一個使用者可以：
- 作為申請人 (applicant) 建立多個預約
- 作為審查人員 (reviewer) 被多個預約引用
- 作為代理人 (delegate) 接手轉送的預約

### Appointment - LeaveSchedule 關係

- 建立 LeaveSchedule 時，必須檢查該時段無預約
- 檢查預約衝突時，需查詢 LeaveSchedule
- 同一審查人員在同一時段內不可同時有預約和休假

### AppointmentHistory 審計追蹤

每次預約狀態變更都會產生一筆歷史記錄，用於：
- 完整的稽核追蹤
- 查詢預約變更歷史
- 合規性報告

---

## 效能考量

### 查詢優化

1. **時段衝突檢測** - 使用複合索引
   ```sql
   SELECT COUNT(*) FROM appointments 
   WHERE reviewer_id = $1 
     AND date = $2 
     AND ((time_start, time_end) OVERLAPS ($3, $4))
     AND status NOT IN ('rejected', 'cancelled');
   ```

2. **月曆檢視** - 使用日期範圍查詢
   ```sql
   SELECT * FROM appointments 
   WHERE reviewer_id = $1 
     AND date BETWEEN $2 AND $3;
   ```

3. **歷史查詢** - 利用審計日誌
   ```sql
   SELECT * FROM appointment_history 
   WHERE appointment_id = $1 
   ORDER BY timestamp DESC;
   ```

### 快取策略

- 審查人員清單快取 (TTL: 1 小時)
- 月曆資料快取 (TTL: 5 分鐘)
- 預約詳情快取 (TTL: 15 分鐘)

---

## 資料完整性約束

1. **唯一性**: ad_account, email 全局唯一
2. **參照完整性**: 所有外鍵必須存在
3. **業務規則**:
   - 申請人 ≠ 審查人員
   - time_start < time_end
   - 工作日、營業時間內
   - 15 分鐘倍數時段