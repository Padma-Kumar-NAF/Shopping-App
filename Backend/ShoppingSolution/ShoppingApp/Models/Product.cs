using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        public Guid StockId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        //[Required]
        //public string CategoryName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation 
        public Category? Category { get; set; } // many to one
        public Stock? Stock { get; set; } // one to one

        public ICollection<OrderDetails>? OrderDetails { get; set; } // one to many
        public ICollection<Review>? Reviews { get; set; } // one to many
        public ICollection<CartItem>? CartItems { get; set; } // one to many


    }
}
