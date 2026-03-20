using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Exceptions;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    public class BaseController : ControllerBase
    {
        protected Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        protected Guid GetUserIdOrThrow()
        {
            var userId = GetUserId();

            if (userId == Guid.Empty)
                throw new AppException("User not authenticated",401);

            return userId;
        }
    }
}