using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IUserController
    {
        public Task<ActionResult<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request);
        public Task<ActionResult<AddUserDetailsResponseDTO>> AddUserDetails(AddUserDetailsRequestDTO request);
    }
}
