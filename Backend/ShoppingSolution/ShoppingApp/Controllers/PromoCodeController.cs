using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Promocode;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "admin,user")]
    [Route("[controller]")]
    [ApiController]
    [ValidateRequest]
    public class PromoCodeController : BaseController
    {
        private readonly IPromoCodeService _promoService;

        public PromoCodeController(IPromoCodeService promoService)
        {
            _promoService = promoService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpPost("add-promocode")]
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
        //[HttpPost("verify-promocode")]
        //[ValidateRequest]
        //public async Task<IActionResult> VerifyPromoCode([FromBody] VerifyPromoCodeRequestDTO request)
        //{
        //    try
        //    {
        //        var result = await _promoService.VerifyPromoCode(request);
        //        return Ok(result);
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        [Authorize(Roles = "admin")]
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

        [Authorize(Roles = "admin")]
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

        [Authorize(Roles = "admin")]
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