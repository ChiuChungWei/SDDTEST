using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractReviewScheduler.Models.Domain
{
    /// <summary>
    /// 休假排程實體
    /// </summary>
    [Table("LeaveSchedules")]
    public class LeaveSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 審查人員 ID
        /// </summary>
        [Required]
        [ForeignKey(nameof(Reviewer))]
        public int ReviewerId { get; set; }

        /// <summary>
        /// 休假日期
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// 休假開始時間
        /// </summary>
        [Required]
        public TimeSpan TimeStart { get; set; }

        /// <summary>
        /// 休假結束時間
        /// </summary>
        [Required]
        public TimeSpan TimeEnd { get; set; }

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

        // Navigation properties
        public virtual User Reviewer { get; set; } = null!;
    }
}
