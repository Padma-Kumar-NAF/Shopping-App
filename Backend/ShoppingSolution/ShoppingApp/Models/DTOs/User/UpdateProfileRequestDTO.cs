using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public class UpdateProfileRequestDTO
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "User details are required.")]
        public UpdateUserDetailsDTO Details { get; set; } = new UpdateUserDetailsDTO();
    }

    public class UpdateUserDetailsDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address Line 1 is required.")]
        [MaxLength(200, ErrorMessage = "Address Line 1 cannot exceed 200 characters.")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address Line 2 is required.")]
        [MaxLength(200, ErrorMessage = "Address Line 2 cannot exceed 200 characters.")]
        public string AddressLine2 { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be a valid 6-digit number.")]
        public string Pincode { get; set; } = string.Empty;
    }
}