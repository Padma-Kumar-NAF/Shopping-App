using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IUserController
    {
        public Task<ActionResult<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request);
        public Task<ActionResult<AddUserDetailsResponseDTO>> AddUserDetails(AddUserDetailsRequestDTO request);
        public Task<ActionResult<UpdateProfileResponseDTO>> UpdateUserDetails(UpdateProfileRequestDTO request);
        public Task<ActionResult<CreateUserResponseDTO>> GetUserById();
            //public Task<ActionResult<GetUsersResponseDTO>> GetUserById(GetUserByIdRequestDTO request);
    }
}
