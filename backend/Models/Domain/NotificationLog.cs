using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractReviewScheduler.Models.Domain
{
    /// <summary>
    /// 通知日誌實體
    /// </summary>
    [Table("NotificationLogs")]
    public class NotificationLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 預約 ID
        /// </summary>
        [Required]
        [ForeignKey(nameof(Appointment))]
        public int AppointmentId { get; set; }

        /// <summary>
        /// 收件人電子郵件
        /// </summary>
        [Required]
        [StringLength(255)]
        public string RecipientEmail { get; set; } = string.Empty;

        /// <summary>
        /// 通知類型 (new_appointment, appointment_confirmed, appointment_rejected, appointment_delegated, delegate_accepted, delegate_rejected)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// 郵件主旨
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 郵件內容
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 發送狀態 (pending, sent, failed)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "pending";

        /// <summary>
        /// 重試次數
        /// </summary>
        [Required]
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 發送時間
        /// </summary>
        public DateTime? SentAt { get; set; }

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
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public virtual Appointment Appointment { get; set; } = null!;
    }
}
