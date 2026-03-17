namespace ShoppingApp.Models.DTOs.Cart
{
    public record RemoveFromCartResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}