using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Promocode
{
    public record AddPromoCodeRequestDTO
    {
        [Required(ErrorMessage = "Promo code name is required")]
        [MaxLength(50, ErrorMessage = "Promo code name cannot exceed 50 characters")]
        public string PromoCodeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount percentage is required")]
        [Range(1, 100, ErrorMessage = "Discount percentage must be between 1 and 100")]
        public int DiscountPercentage { get; set; }

        [Required(ErrorMessage = "From date is required")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required")]
        public DateTime ToDate { get; set; }
    }
}
