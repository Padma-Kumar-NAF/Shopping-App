namespace ShoppingApp.Models.DTOs.Promocode
{
    public class VerifyPromoCodeResponseDTO
    {
        public bool IsValid { get; set; }
        public int DiscountPercentage { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? PromoCodeId { get; set; }
    }
}
