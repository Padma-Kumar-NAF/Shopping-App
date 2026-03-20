using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("edit-user-email")]
        [ValidateRequest]
        public async Task<IActionResult> EditUserMail([FromBody] EditUserEmailRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _userService.EditUserEmail(UserId,request);
                return Ok(Result);
            }
            catch
            {
                throw;
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
                var Result = await _userService.GetAllUsers(request);
                return Ok(Result);
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
                var Result = await _userService.GetUserById(GetUserId());
                return Ok(Result);
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
                var Result = await _userService.UpdateUserDetails(UserId,request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }
    }
}