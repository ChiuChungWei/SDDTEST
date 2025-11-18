using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ContractReviewScheduler.Services;

namespace ContractReviewScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILdapService _ldapService;
        private readonly IJwtService _jwtService;
        private readonly IUserSyncService _userSyncService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ILdapService ldapService,
            IJwtService jwtService,
            IUserSyncService userSyncService,
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _ldapService = ldapService;
            _jwtService = jwtService;
            _userSyncService = userSyncService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.AdAccount) || string.IsNullOrWhiteSpace(request?.Password))
                {
                    return BadRequest(new { error = "帳號和密碼不能為空" });
                }

                // 驗證 LDAP 認證
                var (isValid, errorMessage) = await _ldapService.AuthenticateAsync(
                    request.AdAccount,
                    request.Password);

                if (!isValid)
                {
                    _logger.LogWarning("登入失敗: {AdAccount}, 原因: {Error}", 
                        request.AdAccount, errorMessage);
                    return Unauthorized(new { error = errorMessage ?? "帳號或密碼不正確" });
                }

                // 獲取或建立使用者
                var user = await _userSyncService.GetOrCreateUserAsync(request.AdAccount);
                if (user == null)
                {
                    _logger.LogWarning("無法獲取使用者資訊: {AdAccount}", request.AdAccount);
                    return Unauthorized(new { error = "無法獲取使用者資訊" });
                }

                // 更新最後登入時間
                await _userSyncService.UpdateLastLoginAsync(user.Id);

                // 簽發 JWT Token
                var token = _jwtService.GenerateToken(
                    user.Id,
                    user.AdAccount,
                    user.Role,
                    user.Email);

                _logger.LogInformation("使用者登入成功: {AdAccount}, UserId={UserId}, Role={Role}",
                    request.AdAccount, user.Id, user.Role);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        AdAccount = user.AdAccount,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role
                    },
                    ExpiresIn = 3600
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入異常: {AdAccount}", request?.AdAccount);
                return StatusCode(500, new { error = "登入失敗" });
            }
        }

        /// <summary>
        /// 登出（可選，用於清除客戶端 Token）
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var adAccount = User.FindFirst(ClaimTypes.Name)?.Value ?? "未知使用者";

            _logger.LogInformation("使用者登出: {AdAccount}", adAccount);
            return Ok(new { message = "登出成功" });
        }

        /// <summary>
        /// 獲取目前使用者資訊
        /// </summary>
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim?.Value, out var userId))
                {
                    return Unauthorized(new { error = "無效的使用者身份" });
                }

                var adAccount = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = await _userSyncService.GetOrCreateUserAsync(adAccount);

                if (user == null)
                {
                    return NotFound(new { error = "使用者不存在" });
                }

                return Ok(new UserResponse
                {
                    Id = user.Id,
                    AdAccount = user.AdAccount,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取使用者資訊異常");
                return StatusCode(500, new { error = "獲取使用者資訊失敗" });
            }
        }

        /// <summary>
        /// 驗證 Token 有效性
        /// </summary>
        [HttpPost("verify-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult VerifyToken([FromBody] VerifyTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
            {
                return BadRequest(new { error = "Token 不能為空" });
            }

            var (isValid, principal, error) = _jwtService.ValidateToken(request.Token);

            if (!isValid)
            {
                return Unauthorized(new { error = error ?? "Token 無效" });
            }

            return Ok(new { valid = true, message = "Token 有效" });
        }
    }

    // 請求/回應類別
    public class LoginRequest
    {
        public string? AdAccount { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public UserResponse? User { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class VerifyTokenRequest
    {
        public string? Token { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string? AdAccount { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
