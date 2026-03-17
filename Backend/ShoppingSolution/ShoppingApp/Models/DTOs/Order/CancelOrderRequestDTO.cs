using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public record CancelOrderRequestDTO
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Order Id is required")]
        public Guid OrderId { get; set; }

    }
}
