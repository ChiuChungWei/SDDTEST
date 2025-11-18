using System;
using System.Net;
using Serilog;

namespace ContractReviewScheduler.Middleware
{
    /// <summary>
    /// 角色型存取控制 (RBAC) 授權中間件
    /// </summary>
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;

        // 定義受保護的端點和所需角色
        private readonly Dictionary<string, string[]> _protectedEndpoints = new()
        {
            // 申請人端點
            { "/api/appointments", new[] { "applicant", "reviewer" } },
            { "/api/calendar", new[] { "applicant", "reviewer" } },

            // 審查人員端點
            { "/api/appointments/accept", new[] { "reviewer" } },
            { "/api/appointments/reject", new[] { "reviewer" } },
            { "/api/appointments/delegate", new[] { "reviewer" } },
            { "/api/leave-schedules", new[] { "reviewer" } },

            // 管理端點
            { "/api/users", new[] { "admin" } }
        };

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 檢查是否為受保護端點
            var isProtected = _protectedEndpoints.Keys.Any(endpoint =>
                path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));

            if (isProtected)
            {
                // 檢查使用者是否已認證
                if (!context.User.Identity?.IsAuthenticated ?? false)
                {
                    _logger.LogWarning("未認證的請求嘗試存取受保護端點: {Path}", path);
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "未授權",
                        error = "請先登入"
                    });
                    return;
                }

                // 檢查角色是否符合
                var matchingEndpoint = _protectedEndpoints.FirstOrDefault(e =>
                    path.StartsWith(e.Key, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(matchingEndpoint.Key))
                {
                    var requiredRoles = matchingEndpoint.Value;
                    var userRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                    if (string.IsNullOrEmpty(userRole) || !requiredRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                    {
                        var userName = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "未知使用者";
                        _logger.LogWarning("使用者 {UserName} 權限不足存取 {Path} (角色: {Role})", 
                            userName, path, userRole ?? "無");

                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "禁止存取",
                            error = $"需要以下角色之一: {string.Join(", ", requiredRoles)}"
                        });
                        return;
                    }

                    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "未知";
                    _logger.LogInformation("使用者 {UserId} 已授權存取 {Path} (角色: {Role})", 
                        userId, path, userRole);
                }
            }

            await _next(context);
        }
    }
}
