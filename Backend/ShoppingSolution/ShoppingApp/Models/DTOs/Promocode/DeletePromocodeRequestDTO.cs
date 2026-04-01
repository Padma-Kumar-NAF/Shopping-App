using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Promocode
{
    public record DeletePromocodeRequestDTO
    {
        [Required(ErrorMessage = "PromoCodeId is required")]
        public Guid PromoCodeId { get; set; }
    }
}
