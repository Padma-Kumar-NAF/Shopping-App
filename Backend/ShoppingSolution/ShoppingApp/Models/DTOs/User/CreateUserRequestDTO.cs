using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public class CreateUserRequestDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address Line 1 is required")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address Line 2 is required")]
        public string AddressLine2 { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string Pincode { get; set; } = string.Empty;
    }
}