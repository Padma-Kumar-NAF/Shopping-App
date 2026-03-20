using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public record GetUsersRequestDTO
    {

        [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100.")]
        public int Limit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int PageNumber { get; set; }
    }
}
