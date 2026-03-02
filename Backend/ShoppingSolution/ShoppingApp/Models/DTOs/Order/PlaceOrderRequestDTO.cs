using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public class PlaceOrderRequestDTO
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid AddressId { get; set; }
        [Required]
        public int TotalProductsCount { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public List<PlaceOrderItemDTO> Items { get; set; } = new();
    }

    public class PlaceOrderItemDTO
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public string ProductName { get; set; } = string.Empty;
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal ProductPrice { get; set; }
    }
}
