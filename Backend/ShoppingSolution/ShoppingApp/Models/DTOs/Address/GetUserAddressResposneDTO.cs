using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Models.DTOs.Address
{
    public class GetUserAddressResposneDTO
    {
        public Guid UserId { get; set; }
        public ICollection<AddressDTO> AddressList { get; set; } = new List<AddressDTO>();
    }
}
