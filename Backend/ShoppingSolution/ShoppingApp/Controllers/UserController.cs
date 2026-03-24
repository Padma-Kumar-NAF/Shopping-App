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

        /// <summary>
        /// Updates the email address of the currently authenticated user.
        /// </summary>
        /// <remarks>The user must be authenticated to perform this operation. An exception is thrown if
        /// the user ID cannot be retrieved or if the email update fails.</remarks>
        /// <param name="request">An object containing the new email address and any required validation information for the update operation.</param>
        /// <returns>An IActionResult that indicates the outcome of the email update operation. If successful, the result
        /// includes the updated email information.</returns>
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

        /// <summary>
        /// Retrieves a list of users that match the specified filter and options provided in the request.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized with the 'Admin'
        /// role. The request is validated before processing. Any exceptions encountered are rethrown for further
        /// handling by higher-level middleware.</remarks>
        /// <param name="request">An object containing filter criteria and retrieval options for users. Must not be null.</param>
        /// <returns>An IActionResult containing the list of users that match the request parameters. Returns an empty list if no
        /// users are found.</returns>
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
                var Result = await _userService.GetUserById(UserId);
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

        [HttpPost("delete-user")]
        [ValidateRequest]
        public async Task<IActionResult> DeactivateUser([FromBody] DeleteUserRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _userService.DeactivateUser(UserId,request.UserId);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("change-user-role")]
        [ValidateRequest]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _userService.ChangeUserRole(UserId, request.UserId,request.Status);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }
    }
}