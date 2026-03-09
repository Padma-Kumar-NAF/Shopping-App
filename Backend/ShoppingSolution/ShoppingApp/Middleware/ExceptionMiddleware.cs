using ShoppingApp.Contexts;
using ShoppingApp.Models;
using System.Security.Claims;

namespace ShoppingApp.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ShoppingContext db)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var endpoint = context.GetEndpoint();

                var controller = context.Request.RouteValues["controller"]?.ToString();
                var action = context.Request.RouteValues["action"]?.ToString();

                var user = context.User;

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

                var log = new Log
                {
                    LogId = Guid.NewGuid(),
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? "",
                    UserName = username,
                    Role = role,
                    UserId = userId,
                    Path = context.Request.Path,
                    Controller = controller ?? "",
                    Action = action ?? "",
                    HttpMethod = context.Request.Method,
                    RequestPath = context.Request.Path,
                    CreatedAt = DateTime.UtcNow
                };

                db.Logs.Add(log);
                await db.SaveChangesAsync();

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    message = ex.Message
                });
            }
        }
    }
}