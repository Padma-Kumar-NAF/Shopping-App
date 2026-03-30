namespace ShoppingApp.Models.DTOs.User
{
    public record DeleteUserRequestDTO
    {
        public Guid UserId { get; set; }
    }
}
