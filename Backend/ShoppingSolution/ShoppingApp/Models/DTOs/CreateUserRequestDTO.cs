using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs
{
    public class CreateUserRequestDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

    }
}
