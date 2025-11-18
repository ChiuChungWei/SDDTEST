using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractReviewScheduler.Models.Domain
{
    /// <summary>
    /// 預約實體
    /// </summary>
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 申請人 ID
        /// </summary>
        [Required]
        [ForeignKey(nameof(Applicant))]
        public int ApplicantId { get; set; }

        /// <summary>
        /// 審查人員 ID
        /// </summary>
        [Required]
        [ForeignKey(nameof(Reviewer))]
        public int ReviewerId { get; set; }

        /// <summary>
        /// 預約日期 (YYYY-MM-DD)
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// 預約開始時間 (HH:MM:SS)
        /// </summary>
        [Required]
        public TimeSpan TimeStart { get; set; }

        /// <summary>
        /// 預約結束時間 (HH:MM:SS)
        /// </summary>
        [Required]
        public TimeSpan TimeEnd { get; set; }

        /// <summary>
        /// 契約物件名稱
        /// </summary>
        [Required]
        [StringLength(500)]
        public string ObjectName { get; set; } = string.Empty;

        /// <summary>
        /// 預約狀態 (pending, accepted, rejected, delegated, delegate_accepted, delegate_rejected, cancelled)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "pending";

        /// <summary>
        /// 代理審查人員 ID (若已轉送)
        /// </summary>
        [ForeignKey(nameof(DelegateReviewer))]
        public int? DelegateReviewerId { get; set; }

        /// <summary>
        /// 轉送狀態 (pending, accepted, rejected)
        /// </summary>
        [StringLength(50)]
        public string? DelegateStatus { get; set; }

        /// <summary>
        /// 建立者 ID
        /// </summary>
        [Required]
        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedById { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 取消時間
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// 取消原因
        /// </summary>
        public string? CancelledReason { get; set; }

        // Navigation properties
        public virtual User Applicant { get; set; } = null!;
        public virtual User Reviewer { get; set; } = null!;
        public virtual User? DelegateReviewer { get; set; }
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<AppointmentHistory> Histories { get; set; } = new List<AppointmentHistory>();
        public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    }
}
