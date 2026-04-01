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
        [Required]
        public DateTime DeliveryDate { get; set; }
        //------------------------------------
        [Required]
        public int DiscountPercentage { get; set; } = 0;
        public Guid? PromoCodeId {  get; set; }
        [Required]
        public decimal OrderTotalAmount { get; set; } = 0;
        [Required]
        public decimal DiscountAmount { get; set; } = 0;

        // Navigation
        public PromoCode? PromoCode { get; set; }
        public Address? Address { get; set; } // many-to-one
        public User? User { get; set; } // one to many
        public Payment? Payment { get; set; } // one to one 

        public Refund? Refund { get; set; }

        public ICollection<OrderDetails>? OrderDetails { get; set; }
    }
}
