using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public int TotalProductsCount { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public Guid AddressId { get; set; }


        // Navigation
        public Address? Address { get; set; } // many-to-one

        public User? User { get; set; } // one to many

        public ICollection<OrderDetails>? OrderDetails { get; set; }

        // For now this is field is not needed
        //public Guid OrderDetailsId { get; set; }
    }
}
