using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Address
{
    public record DeleteUserAddressRequestDTO
    {

        [Required(ErrorMessage = "Address Id is required")]
        public Guid AddressId { get; set; }
    }
}
