using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public class AddUserDetailsRequestDTO
    {
        [Required]
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty; // Optional
        public string Email { get; set; } = string.Empty;// Optional
        [Required]
        public string PhoneNumber {  get; set; } = string.Empty;
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
