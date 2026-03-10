using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
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

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ShoppingContext>();

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
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? "",
                    UserName = username,
                    Role = role,
                    UserId = userId,
                    Controller = controller ?? "",
                    Action = action ?? "",
                    HttpMethod = context.Request.Method,
                    RequestPath = context.Request.Path
                };

                db.Logs.Add(log);
                await db.SaveChangesAsync();

                context.Response.StatusCode = ex is AppException appEx ? appEx.StatusCode : 500;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    message = ex is AppException ? ex.Message : "An unexpected error occurred"
                });
            }
        }
    }
}