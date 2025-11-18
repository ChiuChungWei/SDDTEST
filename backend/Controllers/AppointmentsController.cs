using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractReviewScheduler.Services;

namespace ContractReviewScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserSyncService _userSyncService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IUserSyncService userSyncService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _userSyncService = userSyncService;
            _logger = logger;
        }

        /// <summary>
        /// 建立新預約
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
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
                if (request.ReviewerId <= 0)
                {
                    return BadRequest(new { error = "審查人員 ID 無效" });
                }

                if (request.Date < DateTime.Now.Date)
                {
                    return BadRequest(new { error = "預約日期不能在過去" });
                }

                if (string.IsNullOrWhiteSpace(request.ObjectName))
                {
                    return BadRequest(new { error = "契約物件名稱不能為空" });
                }

                // 建立預約
                var (success, appointmentId, error) = await _appointmentService.CreateAppointmentAsync(
                    userId,
                    request.ReviewerId,
                    request.Date,
                    request.StartTime,
                    request.EndTime,
                    request.ObjectName);

                if (!success)
                {
                    _logger.LogWarning("建立預約失敗: {Error}", error);
                    return BadRequest(new { error });
                }

                // 取得建立的預約
                var appointment = await _appointmentService.GetAppointmentAsync(appointmentId.Value);
                return CreatedAtAction(nameof(GetAppointment), 
                    new { id = appointmentId.Value },
                    new AppointmentResponse(appointment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立預約異常");
                return StatusCode(500, new { error = "建立預約失敗" });
            }
        }

        /// <summary>
        /// 取得預約詳情
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentAsync(id);
                if (appointment == null)
                {
                    return NotFound(new { error = "預約不存在" });
                }

                return Ok(new AppointmentResponse(appointment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得預約異常: {AppointmentId}", id);
                return StatusCode(500, new { error = "取得預約失敗" });
            }
        }

        /// <summary>
        /// 接受預約
        /// </summary>
        [HttpPut("{id}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptAppointment(int id)
        {
            try
            {
                // 取得目前使用者
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim?.Value, out var userId))
                {
                    return Unauthorized(new { error = "無效的使用者身份" });
                }

                var (success, error) = await _appointmentService.AcceptAppointmentAsync(id, userId);
                if (!success)
                {
                    return BadRequest(new { error });
                }

                var appointment = await _appointmentService.GetAppointmentAsync(id);
                return Ok(new
                {
                    message = "預約已接受",
                    appointment = new AppointmentResponse(appointment)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接受預約異常: {AppointmentId}", id);
                return StatusCode(500, new { error = "接受預約失敗" });
            }
        }

        /// <summary>
        /// 拒絕預約
        /// </summary>
        [HttpPut("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectAppointment(int id, [FromBody] RejectAppointmentRequest request)
        {
            try
            {
                // 取得目前使用者
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim?.Value, out var userId))
                {
                    return Unauthorized(new { error = "無效的使用者身份" });
                }

                var reason = request?.Reason ?? "未提供原因";
                var (success, error) = await _appointmentService.RejectAppointmentAsync(id, userId, reason);
                if (!success)
                {
                    return BadRequest(new { error });
                }

                return Ok(new { message = "預約已拒絕" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拒絕預約異常: {AppointmentId}", id);
                return StatusCode(500, new { error = "拒絕預約失敗" });
            }
        }
    }

    // 請求/回應類別
    public class CreateAppointmentRequest
    {
        public int ReviewerId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ObjectName { get; set; } = string.Empty;
    }

    public class RejectAppointmentRequest
    {
        public string? Reason { get; set; }
    }

    public class AppointmentResponse
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string? ApplicantName { get; set; }
        public int ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public string ObjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AppointmentResponse(Models.Domain.Appointment? appointment)
        {
            if (appointment == null) return;

            Id = appointment.Id;
            ApplicantId = appointment.ApplicantId;
            ApplicantName = appointment.Applicant?.Name;
            ReviewerId = appointment.ReviewerId;
            ReviewerName = appointment.Reviewer?.Name;
            Date = appointment.Date;
            TimeStart = appointment.TimeStart;
            TimeEnd = appointment.TimeEnd;
            ObjectName = appointment.ObjectName;
            Status = appointment.Status;
            CreatedAt = appointment.CreatedAt;
            UpdatedAt = appointment.UpdatedAt;
        }
    }
}
