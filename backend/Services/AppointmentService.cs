using System;
using Microsoft.EntityFrameworkCore;
using ContractReviewScheduler.Data;
using ContractReviewScheduler.Models.Domain;
using Serilog;

namespace ContractReviewScheduler.Services
{
    /// <summary>
    /// 預約業務邏輯服務
    /// </summary>
    public interface IAppointmentService
    {
        /// <summary>
        /// 建立新預約
        /// </summary>
        Task<(bool Success, int? AppointmentId, string? Error)> CreateAppointmentAsync(
            int applicantId, int reviewerId, DateTime date, TimeSpan startTime, 
            TimeSpan endTime, string objectName);

        /// <summary>
        /// 取得預約詳情
        /// </summary>
        Task<Appointment?> GetAppointmentAsync(int appointmentId);

        /// <summary>
        /// 接受預約
        /// </summary>
        Task<(bool Success, string? Error)> AcceptAppointmentAsync(int appointmentId, int reviewerId);

        /// <summary>
        /// 拒絕預約
        /// </summary>
        Task<(bool Success, string? Error)> RejectAppointmentAsync(int appointmentId, int reviewerId, string reason);

        /// <summary>
        /// 取得評論人的預約清單
        /// </summary>
        Task<List<Appointment>> GetReviewerAppointmentsAsync(int reviewerId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// 取得申請人的預約清單
        /// </summary>
        Task<List<Appointment>> GetApplicantAppointmentsAsync(int applicantId, DateTime? fromDate = null);

        /// <summary>
        /// 驗證時段可用性
        /// </summary>
        Task<(bool IsAvailable, string? Reason)> ValidateAvailabilityAsync(
            int reviewerId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConflictDetectionService _conflictService;
        private readonly ICacheService _cacheService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AppointmentService> _logger;

        // 營業時間: 09:00 - 18:00
        private readonly TimeSpan _businessHourStart = new(9, 0, 0);
        private readonly TimeSpan _businessHourEnd = new(18, 0, 0);

        // 最小時段: 15 分鐘
        private readonly int _timeSlotMinutes = 15;

        public AppointmentService(
            ApplicationDbContext context,
            IConflictDetectionService conflictService,
            ICacheService cacheService,
            IEmailService emailService,
            ILogger<AppointmentService> logger)
        {
            _context = context;
            _conflictService = conflictService;
            _cacheService = cacheService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool Success, int? AppointmentId, string? Error)> CreateAppointmentAsync(
            int applicantId, int reviewerId, DateTime date, TimeSpan startTime, 
            TimeSpan endTime, string objectName)
        {
            try
            {
                // 驗證基本參數
                if (applicantId == reviewerId)
                {
                    return (false, null, "申請人和審查人員不能是同一人");
                }

                if (string.IsNullOrWhiteSpace(objectName))
                {
                    return (false, null, "契約物件名稱不能為空");
                }

                // 驗證日期是否為工作日
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    return (false, null, "只能在工作日預約 (週一至週五)");
                }

                // 驗證時間段
                if (startTime >= endTime)
                {
                    return (false, null, "開始時間必須早於結束時間");
                }

                // 驗證時段是否為 15 分鐘倍數
                if (startTime.Minutes % _timeSlotMinutes != 0 || endTime.Minutes % _timeSlotMinutes != 0)
                {
                    return (false, null, $"時段必須為 {_timeSlotMinutes} 分鐘的倍數");
                }

                // 驗證營業時間
                if (startTime < _businessHourStart || endTime > _businessHourEnd)
                {
                    return (false, null, $"預約時間必須在營業時間內 ({_businessHourStart} - {_businessHourEnd})");
                }

                // 檢查時段可用性
                var (isAvailable, reason) = await ValidateAvailabilityAsync(reviewerId, date, startTime, endTime);
                if (!isAvailable)
                {
                    return (false, null, reason);
                }

                // 建立預約
                var appointment = new Appointment
                {
                    ApplicantId = applicantId,
                    ReviewerId = reviewerId,
                    Date = date,
                    TimeStart = startTime,
                    TimeEnd = endTime,
                    ObjectName = objectName,
                    Status = "pending",
                    CreatedById = applicantId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // 記錄歷史
                var history = new AppointmentHistory
                {
                    AppointmentId = appointment.Id,
                    Action = "created",
                    ActorId = applicantId,
                    Timestamp = DateTime.UtcNow,
                    Notes = "申請人建立預約"
                };

                _context.AppointmentHistories.Add(history);
                await _context.SaveChangesAsync();

                // 清除快取
                _cacheService.Remove(CacheKeys.ReviewerListKey);

                // 發送郵件通知（非同步，不阻塞主流程）
                _ = _emailService.SendAppointmentCreatedNotificationAsync(appointment.Id);

                _logger.LogInformation(
                    "預約已建立: AppointmentId={AppointmentId}, Applicant={Applicant}, Reviewer={Reviewer}, Date={Date}",
                    appointment.Id, applicantId, reviewerId, date);

                return (true, appointment.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立預約異常: Applicant={Applicant}, Reviewer={Reviewer}",
                    applicantId, reviewerId);
                return (false, null, "建立預約失敗");
            }
        }

        public async Task<Appointment?> GetAppointmentAsync(int appointmentId)
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Applicant)
                    .Include(a => a.Reviewer)
                    .Include(a => a.DelegateReviewer)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得預約異常: AppointmentId={AppointmentId}", appointmentId);
                return null;
            }
        }

        public async Task<(bool Success, string? Error)> AcceptAppointmentAsync(int appointmentId, int reviewerId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ReviewerId == reviewerId);

                if (appointment == null)
                {
                    return (false, "預約不存在或您無權操作");
                }

                if (appointment.Status != "pending")
                {
                    return (false, $"只能接受待確認的預約 (當前狀態: {appointment.Status})");
                }

                appointment.Status = "accepted";
                appointment.UpdatedAt = DateTime.UtcNow;

                _context.Appointments.Update(appointment);

                // 記錄歷史
                var history = new AppointmentHistory
                {
                    AppointmentId = appointmentId,
                    Action = "accepted",
                    ActorId = reviewerId,
                    Timestamp = DateTime.UtcNow,
                    Notes = "審查人員已接受預約"
                };

                _context.AppointmentHistories.Add(history);
                await _context.SaveChangesAsync();

                // 發送通知
                _ = _emailService.SendAppointmentAcceptedNotificationAsync(appointmentId);

                _logger.LogInformation("預約已接受: AppointmentId={AppointmentId}, Reviewer={Reviewer}",
                    appointmentId, reviewerId);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接受預約異常: AppointmentId={AppointmentId}", appointmentId);
                return (false, "接受預約失敗");
            }
        }

        public async Task<(bool Success, string? Error)> RejectAppointmentAsync(int appointmentId, int reviewerId, string reason)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ReviewerId == reviewerId);

                if (appointment == null)
                {
                    return (false, "預約不存在或您無權操作");
                }

                if (appointment.Status != "pending")
                {
                    return (false, $"只能拒絕待確認的預約 (當前狀態: {appointment.Status})");
                }

                appointment.Status = "rejected";
                appointment.UpdatedAt = DateTime.UtcNow;

                _context.Appointments.Update(appointment);

                // 記錄歷史
                var history = new AppointmentHistory
                {
                    AppointmentId = appointmentId,
                    Action = "rejected",
                    ActorId = reviewerId,
                    Timestamp = DateTime.UtcNow,
                    Notes = $"審查人員已拒絕: {reason}"
                };

                _context.AppointmentHistories.Add(history);
                await _context.SaveChangesAsync();

                // 發送通知
                _ = _emailService.SendAppointmentRejectedNotificationAsync(appointmentId, reason);

                _logger.LogInformation("預約已拒絕: AppointmentId={AppointmentId}, Reviewer={Reviewer}, Reason={Reason}",
                    appointmentId, reviewerId, reason);

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拒絕預約異常: AppointmentId={AppointmentId}", appointmentId);
                return (false, "拒絕預約失敗");
            }
        }

        public async Task<List<Appointment>> GetReviewerAppointmentsAsync(int reviewerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Appointments
                    .Where(a => a.ReviewerId == reviewerId)
                    .Include(a => a.Applicant)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Date >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(a => a.Date <= toDate.Value);
                }

                return await query.OrderBy(a => a.Date).ThenBy(a => a.TimeStart).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得審查人員預約清單異常: Reviewer={Reviewer}", reviewerId);
                return new List<Appointment>();
            }
        }

        public async Task<List<Appointment>> GetApplicantAppointmentsAsync(int applicantId, DateTime? fromDate = null)
        {
            try
            {
                var query = _context.Appointments
                    .Where(a => a.ApplicantId == applicantId)
                    .Include(a => a.Reviewer)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Date >= fromDate.Value);
                }

                return await query.OrderBy(a => a.Date).ThenBy(a => a.TimeStart).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得申請人預約清單異常: Applicant={Applicant}", applicantId);
                return new List<Appointment>();
            }
        }

        public async Task<(bool IsAvailable, string? Reason)> ValidateAvailabilityAsync(
            int reviewerId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null)
        {
            var (hasConflict, reason) = await _conflictService.CheckConflictAsync(reviewerId, date, startTime, endTime, excludeAppointmentId);
            return (!hasConflict, hasConflict ? reason : null);
        }
    }
}
