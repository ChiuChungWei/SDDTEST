using System.Net;
using System.Text.Json;
using Serilog;

namespace ContractReviewScheduler.Middleware
{
    /// <summary>
    /// 全域例外處理中間件
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "未處理的例外");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                message = "發生錯誤，請稍後重試",
                error = exception.Message,
                timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        message = "請求參數無效",
                        error = argEx.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        message = "未授權",
                        error = "您沒有存取此資源的權限",
                        timestamp = DateTime.UtcNow
                    };
                    break;
                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        message = "資源不存在",
                        error = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
