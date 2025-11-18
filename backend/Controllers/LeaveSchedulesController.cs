using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractReviewScheduler.Data;
using ContractReviewScheduler.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContractReviewScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "reviewer")]
    public class LeaveSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaveSchedulesController> _logger;

        public LeaveSchedulesController(
            ApplicationDbContext context,
            ILogger<LeaveSchedulesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 建立休假排程
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLeaveSchedule([FromBody] CreateLeaveScheduleRequest request)
        {
            try
            {
                // 取得目前使用者
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim?.Value, out var userId))
                {
                    return Unauthorized(new { error = "無效的使用者身份" });
                }

                // 驗證請求
                if (request.Date < DateTime.Now.Date)
                {
                    return BadRequest(new { error = "休假日期不能在過去" });
                }

                if (request.Date.DayOfWeek == DayOfWeek.Saturday || request.Date.DayOfWeek == DayOfWeek.Sunday)
                {
                    return BadRequest(new { error = "只能設定工作日為休假" });
                }

                if (request.StartTime >= request.EndTime)
                {
                    return BadRequest(new { error = "開始時間必須早於結束時間" });
                }

                // 檢查時段是否為 15 分鐘倍數
                if (request.StartTime.Minutes % 15 != 0 || request.EndTime.Minutes % 15 != 0)
                {
                    return BadRequest(new { error = "時段必須為 15 分鐘的倍數" });
                }

                // 檢查是否已存在衝突的休假
                var conflictingLeave = await _context.LeaveSchedules
                    .Where(l => l.ReviewerId == userId && l.Date == request.Date)
                    .Where(l => request.StartTime < l.TimeEnd && request.EndTime > l.TimeStart)
                    .FirstOrDefaultAsync();

                if (conflictingLeave != null)
                {
                    return BadRequest(new { error = "該時段已有休假排程" });
                }

                // 建立休假排程
                var leave = new LeaveSchedule
                {
                    ReviewerId = userId,
                    Date = request.Date,
                    TimeStart = request.StartTime,
                    TimeEnd = request.EndTime,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.LeaveSchedules.Add(leave);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "休假已建立: LeaveId={LeaveId}, Reviewer={Reviewer}, Date={Date}",
                    leave.Id, userId, request.Date);

                return CreatedAtAction(nameof(GetLeaveSchedule),
                    new { id = leave.Id },
                    new LeaveScheduleResponse(leave));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立休假異常");
                return StatusCode(500, new { error = "建立休假失敗" });
            }
        }

        /// <summary>
        /// 取得休假排程詳情
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLeaveSchedule(int id)
        {
            try
            {
                var leave = await _context.LeaveSchedules.FindAsync(id);
                if (leave == null)
                {
                    return NotFound(new { error = "休假排程不存在" });
                }

                return Ok(new LeaveScheduleResponse(leave));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得休假異常: {LeaveId}", id);
                return StatusCode(500, new { error = "取得休假失敗" });
            }
        }

        /// <summary>
        /// 刪除休假排程
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLeaveSchedule(int id)
        {
            try
            {
                // 取得目前使用者
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim?.Value, out var userId))
                {
                    return Unauthorized(new { error = "無效的使用者身份" });
                }

                var leave = await _context.LeaveSchedules.FindAsync(id);
                if (leave == null)
                {
                    return NotFound(new { error = "休假排程不存在" });
                }

                // 檢查是否為該使用者的休假
                if (leave.ReviewerId != userId)
                {
                    return Forbid();
                }

                _context.LeaveSchedules.Remove(leave);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "休假已刪除: LeaveId={LeaveId}, Reviewer={Reviewer}",
                    id, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除休假異常: {LeaveId}", id);
                return StatusCode(500, new { error = "刪除休假失敗" });
            }
        }

        /// <summary>
        /// 取得審查人員的休假清單
        /// </summary>
        [HttpGet("reviewer/{reviewerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReviewerLeaveSchedules(int reviewerId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.LeaveSchedules
                    .Where(l => l.ReviewerId == reviewerId)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(l => l.Date >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(l => l.Date <= toDate.Value);
                }

                var leaves = await query
                    .OrderBy(l => l.Date)
                    .ThenBy(l => l.TimeStart)
                    .ToListAsync();

                return Ok(leaves.Select(l => new LeaveScheduleResponse(l)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得審查人員休假清單異常: {ReviewerId}", reviewerId);
                return StatusCode(500, new { error = "取得休假清單失敗" });
            }
        }
    }

    // 請求/回應類別
    public class CreateLeaveScheduleRequest
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class LeaveScheduleResponse
    {
        public int Id { get; set; }
        public int ReviewerId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public LeaveScheduleResponse(LeaveSchedule leave)
        {
            Id = leave.Id;
            ReviewerId = leave.ReviewerId;
            Date = leave.Date;
            TimeStart = leave.TimeStart;
            TimeEnd = leave.TimeEnd;
            CreatedAt = leave.CreatedAt;
            UpdatedAt = leave.UpdatedAt;
        }
    }
}
