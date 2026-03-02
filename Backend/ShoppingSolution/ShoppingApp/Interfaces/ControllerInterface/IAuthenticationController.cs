using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IAuthenticationController
    {
        public Task<ActionResult<CreateUserResponseDTO>> Register(CreateUserRequestDTO requestDTO);
        public Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO requestDTO);
    }
}
