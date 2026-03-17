using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public record UpdateOrderRequestDTO
    {
        [Required(ErrorMessage = "Order id is required")]
        public Guid OrderId {  get; set; }

        [Required(ErrorMessage = "Order status is required")]
        public string OrderStatus { get; set; } = string.Empty;
    }
}
