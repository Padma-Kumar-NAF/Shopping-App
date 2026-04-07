using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.UserMonthlyProductLimit
{
    public record EditUserMonthlyProductLimitRequestDTO
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Monthly limit is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Monthly limit must be greater than 0")]
        public int MonthlyLimit { get; set; }
    }
}
