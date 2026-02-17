using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Address
    {
        [Key]
        public Guid AddressId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        //[Required]
        //public Guid OrderId { get; set; }

        [Required]
        public string AddressLine1 { get; set; } = string.Empty;

        [Required]
        public string AddressLine2 { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Pincode { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }


        // Navigation
        public User? User { get; set; }
        public ICollection<Order>? Orders { get; set; }

    }
}
