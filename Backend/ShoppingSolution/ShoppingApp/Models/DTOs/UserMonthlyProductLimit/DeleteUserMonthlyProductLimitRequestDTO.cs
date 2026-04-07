using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.UserMonthlyProductLimit
{
    public record DeleteUserMonthlyProductLimitRequestDTO
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
    }
}
