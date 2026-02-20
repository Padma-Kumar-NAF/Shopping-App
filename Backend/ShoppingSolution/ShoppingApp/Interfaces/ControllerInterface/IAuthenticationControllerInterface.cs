using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IAuthenticationControllerInterface
    {
        public Task<ActionResult<CreateUserResponseDTO>> Register(CreateUserRequestDTO requestDTO);
        public Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO requestDTO);
    }
}
