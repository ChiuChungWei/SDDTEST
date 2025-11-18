using System;
using System.Net;
using System.Net.Mail;
using Serilog;

namespace ContractReviewScheduler.Services
{
    /// <summary>
    /// 郵件服務
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// 發送郵件
        /// </summary>
        Task<(bool Success, string? Error)> SendEmailAsync(
            string toAddress, string subject, string body, bool isHtml = true);

        /// <summary>
        /// 發送預約創建通知
        /// </summary>
        Task<bool> SendAppointmentCreatedNotificationAsync(int appointmentId);

        /// <summary>
        /// 發送預約接受通知
        /// </summary>
        Task<bool> SendAppointmentAcceptedNotificationAsync(int appointmentId);

        /// <summary>
        /// 發送預約拒絕通知
        /// </summary>
        Task<bool> SendAppointmentRejectedNotificationAsync(int appointmentId, string reason);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _useSSL;
        private readonly int _maxRetries;
        private readonly int _retryDelaySeconds;

        public EmailService(
            ILogger<EmailService> logger,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;

            _smtpServer = configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            _fromAddress = configuration["Email:FromAddress"] ?? "noreply@company.local";
            _fromName = configuration["Email:FromName"] ?? "Contract Review System";
            _username = configuration["Email:Username"] ?? "";
            _password = configuration["Email:Password"] ?? "";
            _useSSL = bool.Parse(configuration["Email:UseSSL"] ?? "true");
            _maxRetries = int.Parse(configuration["Email:MaxRetries"] ?? "3");
            _retryDelaySeconds = int.Parse(configuration["Email:RetryDelaySeconds"] ?? "60");

            _logger.LogInformation("郵件服務初始化: SmtpServer={SmtpServer}, Port={Port}", 
                _smtpServer, _smtpPort);
        }

        public async Task<(bool Success, string? Error)> SendEmailAsync(
            string toAddress, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(toAddress))
            {
                return (false, "收件人地址不能為空");
            }

            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = _useSSL;
                    client.Credentials = new NetworkCredential(_username, _password);

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_fromAddress, _fromName);
                        message.To.Add(toAddress);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = isHtml;

                        await client.SendMailAsync(message);
                    }
                }

                _logger.LogInformation("郵件已發送: To={ToAddress}, Subject={Subject}", 
                    toAddress, subject);

                return (true, null);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP 異常: To={ToAddress}", toAddress);
                return (false, $"郵件發送失敗: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "郵件發送異常: To={ToAddress}", toAddress);
                return (false, $"郵件發送異常: {ex.Message}");
            }
        }

        public async Task<bool> SendAppointmentCreatedNotificationAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Reviewer)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                {
                    _logger.LogWarning("預約不存在: AppointmentId={AppointmentId}", appointmentId);
                    return false;
                }

                var subject = $"新預約請求 - {appointment.ObjectName}";
                var body = GenerateAppointmentCreatedEmailBody(appointment);

                var (success, error) = await SendEmailAsync(
                    appointment.Reviewer?.Email ?? "",
                    subject,
                    body);

                if (success)
                {
                    // 記錄通知日誌
                    var log = new NotificationLog
                    {
                        AppointmentId = appointmentId,
                        RecipientEmail = appointment.Reviewer?.Email ?? "",
                        NotificationType = "new_appointment",
                        Subject = subject,
                        Content = body,
                        Status = "sent",
                        SentAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.NotificationLogs.Add(log);
                    await _context.SaveChangesAsync();
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送預約創建通知異常: AppointmentId={AppointmentId}", 
                    appointmentId);
                return false;
            }
        }

        public async Task<bool> SendAppointmentAcceptedNotificationAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Applicant)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                {
                    return false;
                }

                var subject = $"預約已接受 - {appointment.ObjectName}";
                var body = GenerateAppointmentAcceptedEmailBody(appointment);

                var (success, error) = await SendEmailAsync(
                    appointment.Applicant?.Email ?? "",
                    subject,
                    body);

                if (success)
                {
                    var log = new NotificationLog
                    {
                        AppointmentId = appointmentId,
                        RecipientEmail = appointment.Applicant?.Email ?? "",
                        NotificationType = "appointment_confirmed",
                        Subject = subject,
                        Content = body,
                        Status = "sent",
                        SentAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.NotificationLogs.Add(log);
                    await _context.SaveChangesAsync();
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送預約接受通知異常: AppointmentId={AppointmentId}", 
                    appointmentId);
                return false;
            }
        }

        public async Task<bool> SendAppointmentRejectedNotificationAsync(int appointmentId, string reason)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Applicant)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                {
                    return false;
                }

                var subject = $"預約已拒絕 - {appointment.ObjectName}";
                var body = GenerateAppointmentRejectedEmailBody(appointment, reason);

                var (success, error) = await SendEmailAsync(
                    appointment.Applicant?.Email ?? "",
                    subject,
                    body);

                if (success)
                {
                    var log = new NotificationLog
                    {
                        AppointmentId = appointmentId,
                        RecipientEmail = appointment.Applicant?.Email ?? "",
                        NotificationType = "appointment_rejected",
                        Subject = subject,
                        Content = body,
                        Status = "sent",
                        SentAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.NotificationLogs.Add(log);
                    await _context.SaveChangesAsync();
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送預約拒絕通知異常: AppointmentId={AppointmentId}", 
                    appointmentId);
                return false;
            }
        }

        private string GenerateAppointmentCreatedEmailBody(Models.Domain.Appointment appointment)
        {
            return $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px; background-color: #f8f9fa; margin-top: 20px; border-radius: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .detail {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>新預約請求</h2>
        </div>
        <div class='content'>
            <p>您好,</p>
            <p>您收到了一個新的預約請求,請查看以下詳情:</p>
            
            <div class='detail'>
                <span class='label'>契約物件:</span> {appointment.ObjectName}
            </div>
            <div class='detail'>
                <span class='label'>申請人:</span> {appointment.Applicant?.Name}
            </div>
            <div class='detail'>
                <span class='label'>預約日期:</span> {appointment.Date:yyyy-MM-dd}
            </div>
            <div class='detail'>
                <span class='label'>預約時間:</span> {appointment.TimeStart:hh\\:mm} - {appointment.TimeEnd:hh\\:mm}
            </div>
            
            <p style='margin-top: 20px;'>請在系統中接受或拒絕此預約。</p>
        </div>
        <div class='footer'>
            <p>契約審查預約系統</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateAppointmentAcceptedEmailBody(Models.Domain.Appointment appointment)
        {
            return $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px; background-color: #f8f9fa; margin-top: 20px; border-radius: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .detail {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>預約已接受 ✓</h2>
        </div>
        <div class='content'>
            <p>您好,</p>
            <p>您的預約已被接受,以下是詳情:</p>
            
            <div class='detail'>
                <span class='label'>契約物件:</span> {appointment.ObjectName}
            </div>
            <div class='detail'>
                <span class='label'>審查人員:</span> {appointment.Reviewer?.Name}
            </div>
            <div class='detail'>
                <span class='label'>預約日期:</span> {appointment.Date:yyyy-MM-dd}
            </div>
            <div class='detail'>
                <span class='label'>預約時間:</span> {appointment.TimeStart:hh\\:mm} - {appointment.TimeEnd:hh\\:mm}
            </div>
            
            <p style='margin-top: 20px;'>預約已確認,感謝您的使用。</p>
        </div>
        <div class='footer'>
            <p>契約審查預約系統</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateAppointmentRejectedEmailBody(Models.Domain.Appointment appointment, string reason)
        {
            return $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px; background-color: #f8f9fa; margin-top: 20px; border-radius: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .detail {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>預約已拒絕</h2>
        </div>
        <div class='content'>
            <p>您好,</p>
            <p>您的預約已被拒絕,以下是詳情:</p>
            
            <div class='detail'>
                <span class='label'>契約物件:</span> {appointment.ObjectName}
            </div>
            <div class='detail'>
                <span class='label'>預約日期:</span> {appointment.Date:yyyy-MM-dd}
            </div>
            <div class='detail'>
                <span class='label'>預約時間:</span> {appointment.TimeStart:hh\\:mm} - {appointment.TimeEnd:hh\\:mm}
            </div>
            <div class='detail'>
                <span class='label'>拒絕原因:</span> {reason}
            </div>
            
            <p style='margin-top: 20px;'>請重新安排預約時間,或聯絡審查人員。</p>
        </div>
        <div class='footer'>
            <p>契約審查預約系統</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
