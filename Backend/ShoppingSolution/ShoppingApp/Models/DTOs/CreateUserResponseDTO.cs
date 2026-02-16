namespace ShoppingApp.Models.DTOs
{
    public class CreateUserResponseDTO
    {
        public bool IsAdmin { get; set; }
        public UserDetails UserDetails { get; set; }

        //// Return the products and other details
    }
}
