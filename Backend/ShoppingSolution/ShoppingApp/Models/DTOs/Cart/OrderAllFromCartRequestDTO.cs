using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public record OrderAllFromCartRequestDTO
    {
        [Required(ErrorMessage = "Cart Id is required")]
        public Guid CartId { get; set; }

        [Required(ErrorMessage = "Address Id is required")]
        public Guid AddressId { get; set; }

        [Required(ErrorMessage = "Payment type is required")]
        public string PaymentType { get; set; } = string.Empty;

        public string PromoCode { get; set; } = string.Empty;

        public bool UseWallet { get; set; } = false;
    }
}
