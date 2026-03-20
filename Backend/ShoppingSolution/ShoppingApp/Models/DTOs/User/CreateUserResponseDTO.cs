using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public record CreateUserResponseDTO
    {
        public bool IsSuccess { get; set; }
        public Guid UserDetailsId { get; set; } 
    }
}
