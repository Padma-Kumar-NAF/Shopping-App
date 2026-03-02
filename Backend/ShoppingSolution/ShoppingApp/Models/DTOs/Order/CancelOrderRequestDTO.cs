using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public class CancelOrderRequestDTO
    {
        [Required]
        public Guid OrderId { get; set; }
    }
}
