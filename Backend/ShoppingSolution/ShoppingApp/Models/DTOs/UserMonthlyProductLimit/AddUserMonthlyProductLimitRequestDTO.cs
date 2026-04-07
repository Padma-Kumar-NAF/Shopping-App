using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.UserMonthlyProductLimit
{
    public record AddUserMonthlyProductLimitRequestDTO
    {
        [Required(ErrorMessage = "Product Id is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Monthly limit is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Monthly limit must be greater than 0")]
        public int MonthlyLimit { get; set; }
    }
}
