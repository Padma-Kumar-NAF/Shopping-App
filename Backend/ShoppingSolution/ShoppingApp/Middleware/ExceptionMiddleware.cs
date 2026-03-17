using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ShoppingApp.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            try
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ShoppingContext>();

                var controller = context.Request.RouteValues["controller"]?.ToString();
                var action = context.Request.RouteValues["action"]?.ToString();
                var user = context.User;

                var (username, role, userId) = ExtractUserDetails(user);

                var requestBody = await ReadRequestBody(context.Request);

                var statusCode = ex is AppException appEx ? appEx.StatusCode : 500;
                var errorMessage = ex is AppException ? ex.Message : "An unexpected error occurred";
                var exceptionType = ex.GetType().Name;
                LogException(_logger, ex, controller, action, username, requestBody);

                var log = new Log
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? "",
                    InnerException = ex.InnerException?.Message ?? "",
                    ExceptionType = exceptionType,
                    UserName = username,
                    Role = role,
                    UserId = userId,
                    Controller = controller ?? "",
                    Action = action ?? "",
                    HttpMethod = context.Request.Method,
                    RequestPath = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    RequestBody = requestBody,
                    CreatedAt = DateTime.UtcNow,
                    StatusCode = statusCode
                };

                db.Logs.Add(log);
                await db.SaveChangesAsync();

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var response = new ErrorResponse
                {
                    Error = GetErrorTitle(statusCode),
                    Message = errorMessage,
                    Timestamp = DateTime.UtcNow,
                    TraceId = context.TraceIdentifier
                };

                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception dbEx)
            {
                _logger.LogCritical(dbEx, "Failed to log exception to database. Original exception: {OriginalMessage}", ex.Message);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    message = "An unexpected error occurred while processing your request",
                    traceId = context.TraceIdentifier
                });
            }
        }

        private (string username, string role, Guid? userId) ExtractUserDetails(ClaimsPrincipal user)
        {
            string username = "Anonymous";
            string role = "Anonymous";
            Guid? userId = null;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                username = user.Identity.Name ?? "Unknown";
                role = user.FindFirst(ClaimTypes.Role)?.Value ?? "User";
                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(idClaim, out Guid parsedId))
                    userId = parsedId;
            }

            return (username, role, userId);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            try
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                if (body.Length > 5000)
                    return "[Request body too large to log]";

                return string.IsNullOrEmpty(body) ? "" : body;
            }
            catch
            {
                return "[Unable to read request body]";
            }
        }

        private void LogException(
            ILogger<ExceptionMiddleware> logger,
            Exception ex,
            string controller,
            string action,
            string username,
            string requestBody)
        {
            logger.LogError(ex,
                "Exception occurred - User: {Username}, Controller: {Controller}, Action: {Action}, Request Body: {RequestBody}",
                username, controller, action, requestBody);
        }

        private string GetErrorTitle(int statusCode) => statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            503 => "Service Unavailable",
            _ => "Error"
        };
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}