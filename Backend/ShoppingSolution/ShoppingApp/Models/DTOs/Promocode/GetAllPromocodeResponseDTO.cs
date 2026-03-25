namespace ShoppingApp.Models.DTOs.Promocode
{
    public class GetAllPromocodeResponseDTO
    {
        public List<PromoCodeItemDTO> PromoCodes { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class PromoCodeItemDTO
    {
        public Guid PromoCodeId { get; set; }
        public string PromoCodeName { get; set; } = string.Empty;
        public int DiscountPercentage { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
