namespace ShoppingApp.Models.DTOs.Cart
{
    public class OrderAllFromCartRequestDTO
    {
        public Guid UserId { get; set; }
        public Guid CartId { get; set; }
        public Guid AddressId { get; set; }
    }
}
