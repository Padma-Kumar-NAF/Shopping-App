using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs
{
    public class CreateUserResponseDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

    }
}
