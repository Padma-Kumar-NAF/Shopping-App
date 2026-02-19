using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class UserHash
    {
        [Key]
        public Guid UserHashId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string HashKey  { get; set; } = string.Empty;

        //[Required]
        //public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
    }
}   