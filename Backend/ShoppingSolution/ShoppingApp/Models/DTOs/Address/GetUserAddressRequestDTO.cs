namespace ShoppingApp.Models.DTOs.Address
{
    public class GetUserAddressRequestDTO
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public Guid UserId { get; set; }
    }
}
