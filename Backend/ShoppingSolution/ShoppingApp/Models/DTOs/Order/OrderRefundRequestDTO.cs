using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public class OrderRefundRequestDTO
    {
        [Required(ErrorMessage = "Order Id is required")]
        public Guid OrderId { get; set; }
        [Required(ErrorMessage = "Payment Id is required")]
        public Guid PaymentId { get; set; }
        [Required(ErrorMessage = "Total amount is required")]
        public decimal TotalAmount { get; set; }
    }
}
