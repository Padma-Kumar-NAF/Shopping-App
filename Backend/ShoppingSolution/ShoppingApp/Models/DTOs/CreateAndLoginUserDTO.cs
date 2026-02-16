namespace ShoppingApp.Models.DTOs
{
    public class CreateAndLoginUserDTO
    {
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
    }
}
