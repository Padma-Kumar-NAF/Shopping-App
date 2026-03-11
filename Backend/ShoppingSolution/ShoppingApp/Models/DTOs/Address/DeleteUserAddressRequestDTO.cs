using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Address
{
    public record DeleteUserAddressRequestDTO
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Address Id is required")]
        public Guid AddressId { get; set; }
    }
}
