using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Address
{
    public class CreateNewAddressRequestDTO
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "AddressLine1 is required")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "AddressLine2 is required")]
        public string AddressLine2 { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "PinCode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PinCode must be 6 digits")]
        public string PinCode { get; set; } = string.Empty;
    }
}