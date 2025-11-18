using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractReviewScheduler.Services;

namespace ContractReviewScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly IConflictDetectionService _conflictService;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(
            IConflictDetectionService conflictService,
            ILogger<CalendarController> logger)
        {
            _conflictService = conflictService;
            _logger = logger;
        }

        /// <summary>
        /// 取得指定審查人員和日期的月曆資訊
        /// </summary>
        [HttpGet("{reviewerId}/{date}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCalendar(int reviewerId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    return BadRequest(new { error = "日期格式無效" });
                }

                if (reviewerId <= 0)
                {
                    return BadRequest(new { error = "審查人員 ID 無效" });
                }

                // 取得可用時段
                var availableSlots = await _conflictService.GetAvailableTimeSlotsAsync(
                    reviewerId, parsedDate, 15);

                var response = new CalendarResponse
                {
                    ReviewerId = reviewerId,
                    Date = parsedDate.Date,
                    AvailableSlots = availableSlots
                        .Select(s => new TimeSlotResponse
                        {
                            Start = s.Start,
                            End = s.End
                        })
                        .ToList()
                };

                _logger.LogInformation(
                    "月曆查詢: ReviewerId={ReviewerId}, Date={Date}, AvailableSlots={Count}",
                    reviewerId, parsedDate, response.AvailableSlots.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "月曆查詢異常: ReviewerId={ReviewerId}, Date={Date}",
                    reviewerId, date);
                return StatusCode(500, new { error = "月曆查詢失敗" });
            }
        }
    }

    // 回應類別
    public class CalendarResponse
    {
        public int ReviewerId { get; set; }
        public DateTime Date { get; set; }
        public List<TimeSlotResponse> AvailableSlots { get; set; } = new();
    }

    public class TimeSlotResponse
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
