using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public class UpdateProfileRequestDTO
    {
        public Guid UserId { get; set; }
        public UpdateUserDetailsDTO Details { get; set; }
    }
    public class UpdateUserDetailsDTO
    {
        public string Name { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string AddressLine1 { get; set; } = string.Empty;
        [Required]
        public string AddressLine2 { get; set; } = string.Empty;
        [Required]
        public string State { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string Pincode { get; set; } = string.Empty;
    }
}
