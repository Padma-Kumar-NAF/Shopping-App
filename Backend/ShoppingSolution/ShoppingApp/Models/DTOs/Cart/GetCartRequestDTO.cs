using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public record GetCartRequestDTO
    {
        public Pagination Pagination { get; set; } = new Pagination();
    }
}
