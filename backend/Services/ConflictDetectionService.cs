using System;
using Microsoft.EntityFrameworkCore;
using ContractReviewScheduler.Data;
using Serilog;

namespace ContractReviewScheduler.Services
{
    /// <summary>
    /// 時段衝突檢測服務
    /// </summary>
    public interface IConflictDetectionService
    {
        /// <summary>
        /// 檢查時段是否有衝突
        /// </summary>
        Task<(bool HasConflict, string? Reason)> CheckConflictAsync(
            int reviewerId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null);

        /// <summary>
        /// 取得可用時段清單（整天）
        /// </summary>
        Task<List<(TimeSpan Start, TimeSpan End)>> GetAvailableTimeSlotsAsync(
            int reviewerId, DateTime date, int slotMinutes = 15);
    }

    public class ConflictDetectionService : IConflictDetectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConflictDetectionService> _logger;

        // 營業時間: 09:00 - 18:00
        private readonly TimeSpan _businessHourStart = new(9, 0, 0);
        private readonly TimeSpan _businessHourEnd = new(18, 0, 0);

        public ConflictDetectionService(ApplicationDbContext context, ILogger<ConflictDetectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool HasConflict, string? Reason)> CheckConflictAsync(
            int reviewerId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null)
        {
            try
            {
                // 檢查預約衝突
                var conflictingAppointment = await _context.Appointments
                    .Where(a => a.ReviewerId == reviewerId && a.Date == date.Date)
                    .Where(a => a.Status != "rejected" && a.Status != "cancelled")
                    .Where(a => excludeAppointmentId == null || a.Id != excludeAppointmentId.Value)
                    .Where(a => 
                        // 時段重疊條件: 新時段的開始時間 < 現有時段的結束時間 AND 新時段的結束時間 > 現有時段的開始時間
                        startTime < a.TimeEnd && endTime > a.TimeStart)
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                {
                    var reason = $"時段衝突: {conflictingAppointment.TimeStart:hh\\:mm} - {conflictingAppointment.TimeEnd:hh\\:mm}";
                    _logger.LogWarning(
                        "預約時段衝突: Reviewer={Reviewer}, Date={Date}, Time={Start}-{End}, ConflictWith={ConflictId}",
                        reviewerId, date, startTime, endTime, conflictingAppointment.Id);
                    
                    return (true, reason);
                }

                // 檢查休假衝突
                var conflictingLeave = await _context.LeaveSchedules
                    .Where(l => l.ReviewerId == reviewerId && l.Date == date.Date)
                    .Where(l => startTime < l.TimeEnd && endTime > l.TimeStart)
                    .FirstOrDefaultAsync();

                if (conflictingLeave != null)
                {
                    var reason = $"時段衝突 (休假): {conflictingLeave.TimeStart:hh\\:mm} - {conflictingLeave.TimeEnd:hh\\:mm}";
                    _logger.LogWarning(
                        "預約與休假衝突: Reviewer={Reviewer}, Date={Date}, Time={Start}-{End}",
                        reviewerId, date, startTime, endTime);
                    
                    return (true, reason);
                }

                _logger.LogDebug(
                    "預約時段驗證成功: Reviewer={Reviewer}, Date={Date}, Time={Start}-{End}",
                    reviewerId, date, startTime, endTime);

                return (false, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "檢查時段衝突異常: Reviewer={Reviewer}, Date={Date}",
                    reviewerId, date);

                return (true, "檢查衝突發生異常");
            }
        }

        public async Task<List<(TimeSpan Start, TimeSpan End)>> GetAvailableTimeSlotsAsync(
            int reviewerId, DateTime date, int slotMinutes = 15)
        {
            var availableSlots = new List<(TimeSpan Start, TimeSpan End)>();

            try
            {
                // 取得當天所有已預約和休假的時段
                var occupiedSlots = new List<(TimeSpan Start, TimeSpan End)>();

                // 取得已預約的時段
                var appointments = await _context.Appointments
                    .Where(a => a.ReviewerId == reviewerId && a.Date == date.Date)
                    .Where(a => a.Status != "rejected" && a.Status != "cancelled")
                    .Select(a => new { a.TimeStart, a.TimeEnd })
                    .ToListAsync();

                foreach (var apt in appointments)
                {
                    occupiedSlots.Add((apt.TimeStart, apt.TimeEnd));
                }

                // 取得休假的時段
                var leaves = await _context.LeaveSchedules
                    .Where(l => l.ReviewerId == reviewerId && l.Date == date.Date)
                    .Select(l => new { l.TimeStart, l.TimeEnd })
                    .ToListAsync();

                foreach (var leave in leaves)
                {
                    occupiedSlots.Add((leave.TimeStart, leave.TimeEnd));
                }

                // 合併重疊的時段
                occupiedSlots = MergeOverlappingSlots(occupiedSlots);

                // 計算可用時段
                var currentTime = _businessHourStart;
                var slotDuration = TimeSpan.FromMinutes(slotMinutes);

                while (currentTime + slotDuration <= _businessHourEnd)
                {
                    var slotEnd = currentTime + slotDuration;

                    // 檢查該時段是否與任何佔用的時段衝突
                    var hasConflict = occupiedSlots.Any(slot =>
                        currentTime < slot.End && slotEnd > slot.Start);

                    if (!hasConflict)
                    {
                        availableSlots.Add((currentTime, slotEnd));
                    }

                    currentTime = currentTime.Add(slotDuration);
                }

                _logger.LogDebug(
                    "可用時段計算完成: Reviewer={Reviewer}, Date={Date}, AvailableSlots={Count}",
                    reviewerId, date, availableSlots.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "計算可用時段異常: Reviewer={Reviewer}, Date={Date}",
                    reviewerId, date);
            }

            return availableSlots;
        }

        /// <summary>
        /// 合併重疊的時段
        /// </summary>
        private List<(TimeSpan Start, TimeSpan End)> MergeOverlappingSlots(
            List<(TimeSpan Start, TimeSpan End)> slots)
        {
            if (slots.Count == 0)
                return slots;

            var sortedSlots = slots.OrderBy(s => s.Start).ToList();
            var merged = new List<(TimeSpan Start, TimeSpan End)> { sortedSlots[0] };

            for (int i = 1; i < sortedSlots.Count; i++)
            {
                var lastMerged = merged[^1];
                var current = sortedSlots[i];

                if (current.Start <= lastMerged.End)
                {
                    // 重疊或相鄰，合併
                    merged[^1] = (lastMerged.Start, TimeSpan.FromTicks(Math.Max(lastMerged.End.Ticks, current.End.Ticks)));
                }
                else
                {
                    // 不重疊，新增
                    merged.Add(current);
                }
            }

            return merged;
        }
    }
}
