namespace ShoppingApp.Models.DTOs.User
{
    public class GetUserByIdResponseDTO
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CreateUserDetailsDTO? UserDetails { get; set; }
    }

    public class CreateUserDetailsDTO
    {
        public Guid UserId { get; set; } // ID of UserDetails
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
    }
}