using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; 

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
