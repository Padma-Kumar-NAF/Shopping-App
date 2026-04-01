namespace ShoppingApp.Models.DTOs.Cart
{
    public record AddToCartResponseDTO
    {
        public Guid CartId { get; set; }
        public Guid CartItemId { get; set; }
    }
}
