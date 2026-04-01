using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Promocode
{
    public record EditPromocodeRequestDTO
    {
        [Required(ErrorMessage = "Promo code Iq reuired")]
        public Guid PromoCodeId { get; set; }
        [Required(ErrorMessage = "PromoCodeName Iq reuired")]
        public string PromoCodeName { get; set; } = string.Empty;
        [Required(ErrorMessage = "DiscountPercentage Iq reuired")]
        public int DiscountPercentage { get; set; }
        [Required(ErrorMessage = "FromDate Iq reuired")]
        public DateTime FromDate { get; set; }
        [Required(ErrorMessage = "ToDate Iq reuired")]
        public DateTime ToDate { get; set; }
    }
}
