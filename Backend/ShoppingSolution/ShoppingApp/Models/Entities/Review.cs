using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }

        [Required]
        public string Summary { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int ReviewPoints { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation
        public User? User { get; set; } // one to many
        public Product? Product { get; set; } // one to many
    }
}
