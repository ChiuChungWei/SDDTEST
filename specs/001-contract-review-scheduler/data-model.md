# 契約審查預約系統 - 資料模型

**日期**: 2025-11-18  
**狀態**: 完成
**技術**: Entity Framework Core Code First

## EF Core Code First 概述

此專案使用 **Entity Framework Core Code First** 開發方式，所有資料庫架構都從 C# 模型自動生成。

### 優點
- 版本控制友善 - 模型變更可直接追蹤
- 類型安全 - 編譯時檢查
- 易於測試 - 可建立記憶體中資料庫
- 自動遷移 - 版本管理資料庫架構

### 工作流程
1. 定義實體模型 (C# 類別)
2. 設定 DbContext
3. 建立遷移: `dotnet ef migrations add <Name>`
4. 套用遷移: `dotnet ef database update`

## C# 實體模型定義

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

## C# POCO 實體定義

使用 POCO (Plain Old CLR Objects) 物件定義，無需 AutoMapper，直接映射 SQL Server。

### User 實體

```csharp
using System;
using System.Collections.Generic;

namespace ContractReviewScheduler.Models
{
    /// <summary>
    /// 系統使用者 - 申請人或審查人員
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        
        /// <summary>Active Directory 帳號</summary>
        public string AdAccount { get; set; }
        
        /// <summary>使用者全名</summary>
        public string Name { get; set; }
        
        /// <summary>電子郵件，格式: {ad_account}@isn.co.jp</summary>
        public string Email { get; set; }
        
        /// <summary>角色: applicant 或 reviewer</summary>
        public string Role { get; set; }
        
        /// <summary>帳號啟用狀態</summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>最後登入時間</summary>
        public DateTime? LastLoginAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Appointment> AppointmentsAsApplicant { get; set; }
        public virtual ICollection<Appointment> AppointmentsAsReviewer { get; set; }
        public virtual ICollection<Appointment> AppointmentsAsDelegate { get; set; }
        public virtual ICollection<LeaveSchedule> LeaveSchedules { get; set; }
        public virtual ICollection<AppointmentHistory> HistoriesAsActor { get; set; }
        public virtual ICollection<NotificationLog> NotificationLogs { get; set; }
    }
}
```

### Appointment 實體

```csharp
using System;
using System.Collections.Generic;

namespace ContractReviewScheduler.Models
{
    /// <summary>
    /// 預約 - 申請人與審查人員的預約記錄
    /// </summary>
    public class Appointment
    {
        public Guid Id { get; set; }
        
        /// <summary>申請人 ID</summary>
        public Guid ApplicantId { get; set; }
        
        /// <summary>審查人員 ID</summary>
        public Guid ReviewerId { get; set; }
        
        /// <summary>預約日期</summary>
        public DateOnly Date { get; set; }
        
        /// <summary>預約開始時間</summary>
        public TimeOnly TimeStart { get; set; }
        
        /// <summary>預約結束時間</summary>
        public TimeOnly TimeEnd { get; set; }
        
        /// <summary>契約物件名稱</summary>
        public string ObjectName { get; set; }
        
        /// <summary>預約狀態</summary>
        public string Status { get; set; } = "pending";
        
        /// <summary>代理審查人員 ID (若已轉送)</summary>
        public Guid? DelegateReviewerId { get; set; }
        
        /// <summary>轉送狀態</summary>
        public string DelegateStatus { get; set; }
        
        /// <summary>建立者 ID</summary>
        public Guid CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>取消時間</summary>
        public DateTime? CancelledAt { get; set; }
        
        /// <summary>取消原因</summary>
        public string CancelledReason { get; set; }

        // Navigation properties
        public virtual User Applicant { get; set; }
        public virtual User Reviewer { get; set; }
        public virtual User DelegateReviewer { get; set; }
        public virtual User CreatedByUser { get; set; }
        public virtual ICollection<AppointmentHistory> Histories { get; set; }
        public virtual ICollection<NotificationLog> NotificationLogs { get; set; }
    }
}
```

### LeaveSchedule 實體

```csharp
using System;

namespace ContractReviewScheduler.Models
{
    /// <summary>
    /// 審查人員的休假排程
    /// </summary>
    public class LeaveSchedule
    {
        public Guid Id { get; set; }
        
        /// <summary>審查人員 ID</summary>
        public Guid ReviewerId { get; set; }
        
        /// <summary>休假日期</summary>
        public DateOnly Date { get; set; }
        
        /// <summary>休假開始時間</summary>
        public TimeOnly TimeStart { get; set; }
        
        /// <summary>休假結束時間</summary>
        public TimeOnly TimeEnd { get; set; }
        
        /// <summary>休假原因</summary>
        public string Reason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User Reviewer { get; set; }
    }
}
```

### AppointmentHistory 實體

```csharp
using System;

namespace ContractReviewScheduler.Models
{
    /// <summary>
    /// 預約歷史記錄 - 稽核追蹤用
    /// </summary>
    public class AppointmentHistory
    {
        public Guid Id { get; set; }
        
        /// <summary>預約 ID</summary>
        public Guid AppointmentId { get; set; }
        
        /// <summary>執行的動作</summary>
        public string Action { get; set; }
        
        /// <summary>執行者 ID</summary>
        public Guid ActorId { get; set; }
        
        /// <summary>變更前狀態</summary>
        public string OldStatus { get; set; }
        
        /// <summary>變更後狀態</summary>
        public string NewStatus { get; set; }
        
        /// <summary>額外詳情 (JSON)</summary>
        public string Details { get; set; }
        
        /// <summary>操作時間</summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>備註</summary>
        public string Notes { get; set; }

        // Navigation properties
        public virtual Appointment Appointment { get; set; }
        public virtual User Actor { get; set; }
    }
}
```

### NotificationLog 實體

```csharp
using System;

namespace ContractReviewScheduler.Models
{
    /// <summary>
    /// 通知紀錄 - 稽核用
    /// </summary>
    public class NotificationLog
    {
        public Guid Id { get; set; }
        
        /// <summary>相關預約 ID</summary>
        public Guid AppointmentId { get; set; }
        
        /// <summary>收件人 ID</summary>
        public Guid RecipientId { get; set; }
        
        /// <summary>通知類型</summary>
        public string NotificationType { get; set; }
        
        /// <summary>發送狀態</summary>
        public string Status { get; set; } = "pending";
        
        /// <summary>發送時間</summary>
        public DateTime? SentAt { get; set; }
        
        /// <summary>重試次數</summary>
        public int RetryCount { get; set; } = 0;
        
        /// <summary>錯誤訊息</summary>
        public string ErrorMessage { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Appointment Appointment { get; set; }
        public virtual User Recipient { get; set; }
    }
}
```

## DbContext 配置

### ApplicationDbContext

```csharp
using Microsoft.EntityFrameworkCore;
using ContractReviewScheduler.Models;

namespace ContractReviewScheduler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<LeaveSchedule> LeaveSchedules { get; set; }
        public DbSet<AppointmentHistory> AppointmentHistories { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 配置
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                
                entity.HasIndex(e => e.AdAccount).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.AdAccount).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Appointment 配置
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                
                // 複合索引
                entity.HasIndex(e => new { e.ReviewerId, e.Date, e.TimeStart, e.TimeEnd })
                    .HasName("idx_appointments_reviewer_date");
                entity.HasIndex(e => new { e.ApplicantId, e.CreatedAt })
                    .HasName("idx_appointments_applicant_created");
                entity.HasIndex(e => new { e.Status, e.CreatedAt })
                    .HasName("idx_appointments_status_created");
                
                entity.Property(e => e.ObjectName).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DelegateStatus).HasMaxLength(50);
                
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // 外鍵關係
                entity.HasOne(e => e.Applicant)
                    .WithMany(u => u.AppointmentsAsApplicant)
                    .HasForeignKey(e => e.ApplicantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reviewer)
                    .WithMany(u => u.AppointmentsAsReviewer)
                    .HasForeignKey(e => e.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DelegateReviewer)
                    .WithMany(u => u.AppointmentsAsDelegate)
                    .HasForeignKey(e => e.DelegateReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LeaveSchedule 配置
            modelBuilder.Entity<LeaveSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                
                entity.HasIndex(e => new { e.ReviewerId, e.Date, e.TimeStart, e.TimeEnd })
                    .HasName("idx_leave_schedules_reviewer_date");
                
                entity.Property(e => e.Reason).HasMaxLength(255);
                
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Reviewer)
                    .WithMany(u => u.LeaveSchedules)
                    .HasForeignKey(e => e.ReviewerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AppointmentHistory 配置
            modelBuilder.Entity<AppointmentHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                
                entity.HasIndex(e => new { e.AppointmentId, e.Timestamp })
                    .HasName("idx_appointment_history_appointment_timestamp");
                entity.HasIndex(e => new { e.ActorId, e.Timestamp })
                    .HasName("idx_appointment_history_actor_timestamp");
                
                entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
                entity.Property(e => e.OldStatus).HasMaxLength(50);
                entity.Property(e => e.NewStatus).HasMaxLength(50);
                entity.Property(e => e.Details).HasColumnType("nvarchar(max)");
                
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Appointment)
                    .WithMany(a => a.Histories)
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Actor)
                    .WithMany(u => u.HistoriesAsActor)
                    .HasForeignKey(e => e.ActorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NotificationLog 配置
            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                
                entity.Property(e => e.NotificationType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ErrorMessage).HasColumnType("nvarchar(max)");
                
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Appointment)
                    .WithMany(a => a.NotificationLogs)
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Recipient)
                    .WithMany(u => u.NotificationLogs)
                    .HasForeignKey(e => e.RecipientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
```

## EF Core 遷移工作流程

### 建立初始遷移

```powershell
# 在 Package Manager Console 或終端執行
dotnet ef migrations add InitialCreate
```

### 套用遷移至資料庫

```powershell
dotnet ef database update
```

### 檢視 SQL 指令

```powershell
# 查看將要執行的 SQL
dotnet ef migrations script InitialCreate
```

### 建立新的資料庫變更遷移

```powershell
# 修改模型後
dotnet ef migrations add AddNewField
dotnet ef database update
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