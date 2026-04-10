namespace ShoppingApp.Models.DTOs.UserPromoCodes
{
    public record GetAllUserPromoCodesResponseDTO
    {
        public List<UserPromoCodeItemDTO> PromoCodes { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public record UserPromoCodeItemDTO
    {
        public Guid PromoCodeId { get; set; }
        public string PromoCodeName { get; set; } = string.Empty;
        public int DiscountPercentage { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
