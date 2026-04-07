namespace ShoppingApp.Models.DTOs.UserMonthlyProductLimit
{
    public record GetAllUserMonthlyProductLimitResponseDTO
    {
        public ICollection<UserMonthlyProductLimitDTO> Records { get; set; } = new List<UserMonthlyProductLimitDTO>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public record UserMonthlyProductLimitDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int MonthlyLimit { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
