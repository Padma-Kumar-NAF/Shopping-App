namespace ShoppingApp.Models.DTOs.User
{
    public record GetUserByIdRequestDTO
    {
        public Guid UserId { get; set; }
    }
}
