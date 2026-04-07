using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.Services;
using ShoppingApp.Models.DTOs.UserMonthlyProductLimit;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("[controller]")]
    [ApiController]
    public class UserMonthlyProductLimitController : BaseController
    {
        private readonly IUserMonthlyProductLimit _userMonthlyProductLimit;

        public UserMonthlyProductLimitController(IUserMonthlyProductLimit userMonthlyProductLimit)
        {
            _userMonthlyProductLimit = userMonthlyProductLimit;
        }

        /// <summary>
        /// Adds a new monthly product limit record.
        /// </summary>
        /// <param name="request">An object containing the product ID and monthly limit value.</param>
        /// <returns>An IActionResult containing the ID of the newly created record.</returns>
        [HttpPut("add-limit")]
        [ValidateRequest]
        public async Task<IActionResult> AddLimit([FromBody] AddUserMonthlyProductLimitRequestDTO request)
        {
            try
            {
                var result = await _userMonthlyProductLimit.AddLimit(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates an existing monthly product limit record.
        /// </summary>
        /// <param name="request">An object containing the record ID and the new monthly limit value.</param>
        /// <returns>An IActionResult indicating whether the update was successful.</returns>
        [HttpPost("edit-limit")]
        [ValidateRequest]
        public async Task<IActionResult> EditLimit([FromBody] EditUserMonthlyProductLimitRequestDTO request)
        {
            try
            {
                var result = await _userMonthlyProductLimit.EditLimit(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes an existing monthly product limit record by ID.
        /// </summary>
        /// <param name="request">An object containing the ID of the record to delete.</param>
        /// <returns>An IActionResult indicating whether the deletion was successful.</returns>
        [HttpDelete("delete-limit")]
        [ValidateRequest]
        public async Task<IActionResult> DeleteLimit([FromBody] DeleteUserMonthlyProductLimitRequestDTO request)
        {
            try
            {
                var result = await _userMonthlyProductLimit.DeleteLimit(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves all monthly product limit records with pagination.
        /// </summary>
        /// <param name="request">An object containing pagination options (page number and page size).</param>
        /// <returns>An IActionResult containing a paginated list of monthly product limit records.</returns>
        [HttpPost("get-limits")]
        [ValidateRequest]
        public async Task<IActionResult> GetAllLimits([FromBody] GetAllUserMonthlyProductLimitRequestDTO request)
        {
            try
            {
                var result = await _userMonthlyProductLimit.GetAllLimits(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}
