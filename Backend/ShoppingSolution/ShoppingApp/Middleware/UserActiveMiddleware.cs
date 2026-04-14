using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using System.Security.Claims;

namespace ShoppingApp.Middleware
{
    public class UserActiveMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActiveMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            var path = context.Request.Path.Value;

            if (path.Contains("/login") || path.Contains("/register"))
            {
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    Guid userId = Guid.Parse(userIdClaim.Value);

                    var user = await userService.GetUserById(userId);

                    if (user == null || !user.Data.Active)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("User is inactive");
                        return;
                    }
                }
            }
            await _next(context);
        }
    }
}
