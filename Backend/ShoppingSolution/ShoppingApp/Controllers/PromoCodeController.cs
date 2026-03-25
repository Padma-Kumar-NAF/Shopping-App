using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Promocode;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PromoCodeController : BaseController
    {
        private readonly IPromoCodeService _promoService;

        public PromoCodeController(IPromoCodeService promoService)
        {
            _promoService = promoService;
        }

        /// <summary>
        /// Add new promo code
        /// </summary>
        [HttpPost("add-promocode")]
        [ValidateRequest]
        public async Task<IActionResult> AddPromoCode([FromBody] AddPromoCodeRequestDTO request)
        {
            try
            {
                var result = await _promoService.AddPromoCode(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Verify promo code
        /// </summary>
        [HttpPost("verify-promocode")]
        [ValidateRequest]
        public async Task<IActionResult> VerifyPromoCode([FromBody] VerifyPromoCodeRequestDTO request)
        {
            try
            {
                var result = await _promoService.VerifyPromoCode(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("get-all-promocode")]
        [ValidateRequest]
        public async Task<IActionResult> GetAllPromoCodes([FromBody] GetAllPromocodeRequestDTO request)
        {
            try
            {
                var result = await _promoService.GetAllPromocode(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("edit-promocode")]
        [ValidateRequest]
        public async Task<IActionResult> EditPromoCodes([FromBody] EditPromocodeRequestDTO request)
        {
            try
            {
                var result = await _promoService.EditPromoCode(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("delete-promocode")]
        [ValidateRequest]
        public async Task<IActionResult> DeletePromoCode([FromBody] DeletePromocodeRequestDTO request)
        {
            try
            {
                var result = await _promoService.DeletePromoCode(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}