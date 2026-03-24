using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Wallet;

namespace ShoppingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : BaseController
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        [ValidateRequest]
        public async Task<ActionResult<GetWalletAmountResponseDTO>> GetWalletBalance()
        {
            try
            {
                var userId = GetUserId();
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