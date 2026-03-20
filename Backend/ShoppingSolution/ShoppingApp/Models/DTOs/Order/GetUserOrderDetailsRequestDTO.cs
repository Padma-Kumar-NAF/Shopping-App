using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public record GetUserOrderDetailsRequestDTO
    {
        public Pagination pagination { get; set; }  = new Pagination();
    }
}
