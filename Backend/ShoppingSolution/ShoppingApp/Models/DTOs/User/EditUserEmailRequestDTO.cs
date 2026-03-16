using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public class EditUserEmailRequestDTO
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string OldEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string NewEmail { get; set; } = string.Empty;
    }
}
