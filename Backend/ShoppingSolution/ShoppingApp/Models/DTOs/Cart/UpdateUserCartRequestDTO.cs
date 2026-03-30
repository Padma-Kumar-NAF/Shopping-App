namespace ShoppingApp.Models.DTOs.Cart
{
    public record UpdateUserCartRequestDTO
    {
        public Guid CartId { get; set; }
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
