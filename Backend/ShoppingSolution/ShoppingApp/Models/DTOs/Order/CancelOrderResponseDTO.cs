namespace ShoppingApp.Models.DTOs.Order
{
    public record CancelOrderResponseDTO
    {
        public bool IsSuccess { get; set; }
        public decimal RefuncdAmount { get; set; }
    }
}
