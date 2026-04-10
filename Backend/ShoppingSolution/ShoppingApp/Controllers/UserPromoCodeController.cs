using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.Services;
using ShoppingApp.Models.DTOs.Promocode;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "user")]
    [Route("[controller]")]
    [ApiController]
    [ValidateRequest]
    public class UserPromoCodeController : BaseController
    {
        private readonly IUserPromoCodeService _userPromoCodeService;
        public UserPromoCodeController(IUserPromoCodeService userPromoCodeService)
        {
            _userPromoCodeService = userPromoCodeService;
        }

        [HttpPost("verify-promocode")]
        [ValidateRequest]
        public async Task<IActionResult> VerifyPromoCode([FromBody] VerifyPromoCodeRequestDTO request)
        {
            try
            {
                var result = await _userPromoCodeService.VerifyPromoCode(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("get-user-promocodes")]
        [ValidateRequest]
        public async Task<IActionResult> GetAllUserPromoCodes()
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _userPromoCodeService.GetAllUserPromoCodes(UserId);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

    }
}
