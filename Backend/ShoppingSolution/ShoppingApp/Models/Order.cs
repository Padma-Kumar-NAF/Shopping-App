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
        public bool IsDelivered { get; set; }

        [Required]
        public int TotalProductsCount { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // For now this is field is not needed
        //public Guid OrderDetailsId { get; set; }
    }
}
