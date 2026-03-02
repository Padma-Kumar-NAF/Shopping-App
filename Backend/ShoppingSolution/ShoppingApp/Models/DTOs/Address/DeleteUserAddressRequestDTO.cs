using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Address
{
    public class DeleteUserAddressRequestDTO
    {
        public Guid UserId { get; set; }
        [Required]
        public Guid AddressId { get; set; }
    }
}
