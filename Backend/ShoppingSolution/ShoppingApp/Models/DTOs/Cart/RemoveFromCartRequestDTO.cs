namespace ShoppingApp.Models.DTOs.Cart
{
    public class RemoveFromCartRequestDTO
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
    }
}
