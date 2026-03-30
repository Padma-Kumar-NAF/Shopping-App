namespace ShoppingApp.Models.DTOs.Promocode
{
    public record VerifyPromoCodeRequestDTO
    {
        public string PromoCodeName { get; set; } = string.Empty;
    }
}
