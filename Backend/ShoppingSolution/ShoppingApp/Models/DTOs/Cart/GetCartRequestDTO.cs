namespace ShoppingApp.Models.DTOs.Cart
{
    public class GetCartRequestDTO
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public Guid UserId { get; set; }
    }
}
