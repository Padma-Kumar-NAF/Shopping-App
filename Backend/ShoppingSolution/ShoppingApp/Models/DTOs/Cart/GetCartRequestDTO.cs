using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public class GetCartRequestDTO
    {
        [Required]
        public int Limit { get; set; }
        [Required]
        public int PageNumber { get; set; }
        [Required]
        public Guid UserId { get; set; }
    }
}
