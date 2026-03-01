namespace ShoppingApp.Models.DTOs.Cart
{
    public class RemoveFromCartRequestDTO
    {
        public Guid UserId { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
    }
}
