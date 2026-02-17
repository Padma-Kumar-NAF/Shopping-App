using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class OrderDetails
    {
        [Key]
        public Guid OrderDetailsId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public decimal ProductPrice { get; set; }

        public ICollection<User>? Users { get; set; }
        public ICollection<Product>? Products { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
