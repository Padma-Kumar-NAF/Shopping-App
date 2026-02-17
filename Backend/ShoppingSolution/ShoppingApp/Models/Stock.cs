using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Stock
    {
        [Key]
        public Guid StockId {  get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string ProductName{ get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
