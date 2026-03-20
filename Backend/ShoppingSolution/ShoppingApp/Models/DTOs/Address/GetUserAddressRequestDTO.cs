using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Address
{
    public record GetUserAddressRequestDTO
    {
        public Pagination Pagination {  get; set; } = new Pagination();
    }
}
