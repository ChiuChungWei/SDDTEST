using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractReviewScheduler.Models.Domain
{
    /// <summary>
    /// 預約歷史記錄實體
    /// </summary>
    [Table("AppointmentHistories")]
    public class AppointmentHistory
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
        /// 操作名稱 (created, accepted, rejected, delegated, delegate_accepted, delegate_rejected, cancelled)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 操作者 ID
        /// </summary>
        [Required]
        [ForeignKey(nameof(Actor))]
        public int ActorId { get; set; }

        /// <summary>
        /// 操作時間戳記
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 備註
        /// </summary>
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual User Actor { get; set; } = null!;
    }
}
