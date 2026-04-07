namespace ShoppingApp.Models.DTOs.UserMonthlyProductLimit
{
    public record GetAllUserMonthlyProductLimitRequestDTO
    {
        public Pagination Pagination { get; set; } = new Pagination();
    }
}
