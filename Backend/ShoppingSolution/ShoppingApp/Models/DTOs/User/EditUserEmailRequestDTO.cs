namespace ShoppingApp.Models.DTOs.User
{
    public class EditUserEmailRequestDTO
    {
        public Guid UserId { get; set; }
        public string Password { get; set; } = string.Empty;
        public string OldEmail { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
    }
}
