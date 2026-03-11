using Microsoft.AspNetCore.Mvc;
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
    }
}
