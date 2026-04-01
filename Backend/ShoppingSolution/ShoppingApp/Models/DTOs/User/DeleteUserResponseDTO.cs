namespace ShoppingApp.Models.DTOs.User
{
    public record DeleteUserResponseDTO
    {
        public bool UnActivated{ get; set; } = false;
    }
}
