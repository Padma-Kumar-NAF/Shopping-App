using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public class UpdateProfileRequestDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "User details are required.")]
        public UpdateUserDetailsDTO Details { get; set; } = new UpdateUserDetailsDTO();
    }

    public class UpdateUserDetailsDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address Line 1 is required.")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address Line 2 is required.")]
        public string AddressLine2 { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits.")]
        public string Pincode { get; set; } = string.Empty;
    }
}