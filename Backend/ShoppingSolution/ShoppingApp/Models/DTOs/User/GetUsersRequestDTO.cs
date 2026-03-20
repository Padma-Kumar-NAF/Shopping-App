using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public record GetUsersRequestDTO
    {
        public Pagination Pagination { get; set; } = new Pagination();
    }
}
