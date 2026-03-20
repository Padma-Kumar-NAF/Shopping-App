using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;
using System.Security.Claims;

namespace ShoppingApp.Controllers 
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UserController : BaseController, IUserController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, IUserDetailsService userDetailsService)
        {
            _userService = userService;
        }

        //[Authorize(Roles = "User")]
        [HttpPost("add-user-details")]
        [ValidateRequest]
        public async Task<IActionResult> AddUserDetails([FromBody] AddUserDetailsRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _userService.AddUserDetails(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit-user-email")]
        [ValidateRequest]
        public async Task<IActionResult> EditUserMail([FromBody] EditUserEmailRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _userService.EditUserEmail(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("get-all-users")]
        [ValidateRequest]
        public async Task<IActionResult> GetAllUsers([FromBody] GetUsersRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _userService.GetAllUsers(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("get-user-by-id")]
        [ValidateRequest]
        public async Task<IActionResult> GetUserById()
        { 
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _userService.GetUserById(GetUserId());
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("update-user-details")]
        [ValidateRequest]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateProfileRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _userService.UpdateUserDetails(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}