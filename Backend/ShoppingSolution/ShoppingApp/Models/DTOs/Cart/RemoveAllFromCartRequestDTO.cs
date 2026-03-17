namespace ShoppingApp.Models.DTOs.Cart
{
    public record RemoveAllFromCartRequestDTO
    {
        public Guid UserId { get; set; }
    }
}
