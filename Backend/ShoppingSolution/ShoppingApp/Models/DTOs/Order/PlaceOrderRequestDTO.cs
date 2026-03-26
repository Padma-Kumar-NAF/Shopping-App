using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public record PlaceOrderRequestDTO
    {
        [Required(ErrorMessage = "Address Id is required")]
        public Guid AddressId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Total products count must be greater than 0")]
        public int TotalProductsCount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Order Product is required")]
        public PlaceOrderItemDTO OrderProductdDetails { get; set; } = new PlaceOrderItemDTO();

        [Required(ErrorMessage = "Payment type is required")]
        public string PaymentType { get; set; } = string.Empty;

        public string PromoCode { get; set; } = string.Empty;

        /// <summary>Whether to deduct from the user's wallet balance.</summary>
        public bool UseWallet { get; set; } = false;

        /// <summary>Stripe PaymentMethod ID returned from the frontend after successful Stripe payment.</summary>
        public string StripePaymentId { get; set; } = string.Empty;
    }

    public class PlaceOrderItemDTO
    {
        [Required(ErrorMessage = "Product Id is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [MinLength(2, ErrorMessage = "Product name must be at least 2 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than 0")]
        public decimal ProductPrice { get; set; }
    }
}