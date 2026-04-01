using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Wallet;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "user")]
    [Route("api/[controller]")]
    [ApiController]
    [ValidateRequest]

    public class WalletController : BaseController
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var result = await _walletService.GetWalletAmount(userId);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}