using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public record CancelOrderRequestDTO
    {

        [Required(ErrorMessage = "Order Id is required")]
        public Guid OrderId { get; set; }
    }
}
