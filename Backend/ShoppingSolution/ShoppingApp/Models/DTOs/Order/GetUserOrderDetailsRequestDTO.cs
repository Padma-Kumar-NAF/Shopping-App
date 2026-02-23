namespace ShoppingApp.Models.DTOs.Order
{
    public class GetUserOrderDetailsRequestDTO
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public Guid UserId { get; set; }
    }
}
