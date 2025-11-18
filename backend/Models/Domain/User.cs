using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractReviewScheduler.Models.Domain
{
    /// <summary>
    /// 系統使用者實體
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Active Directory 帳號
        /// </summary>
        [Required]
        [StringLength(100)]
        public string AdAccount { get; set; } = string.Empty;

        /// <summary>
        /// 使用者全名
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 電子郵件地址
        /// </summary>
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 使用者角色 (applicant/reviewer)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "applicant";

        /// <summary>
        /// 帳號啟用狀態
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 最後登入時間
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

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
        public virtual ICollection<Appointment> ApplicantAppointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Appointment> ReviewerAppointments { get; set; } = new List<Appointment>();
        public virtual ICollection<LeaveSchedule> LeaveSchedules { get; set; } = new List<LeaveSchedule>();
        public virtual ICollection<AppointmentHistory> AppointmentHistories { get; set; } = new List<AppointmentHistory>();
    }
}
