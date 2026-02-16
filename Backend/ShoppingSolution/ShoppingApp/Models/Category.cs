using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int CategoryName { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
