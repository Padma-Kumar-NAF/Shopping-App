using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public record CreateUserResponseDTO
    {
        public bool isSuccess { get; set; }
    }
}
