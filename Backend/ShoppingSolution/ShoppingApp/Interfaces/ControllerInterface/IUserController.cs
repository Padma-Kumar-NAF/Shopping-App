using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IUserController
    {
        public Task<IActionResult> GetAllUsers(GetUsersRequestDTO request);
        //public Task<IActionResult> AddUserDetails(AddUserDetailsRequestDTO request);
        public Task<IActionResult> UpdateUserDetails(UpdateProfileRequestDTO request);
        public Task<IActionResult> GetUserById();
        public Task<IActionResult> EditUserMail(EditUserEmailRequestDTO request);
    }
}