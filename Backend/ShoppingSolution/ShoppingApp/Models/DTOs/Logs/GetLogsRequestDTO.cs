namespace ShoppingApp.Models.DTOs.Logs
{
    public record GetLogsRequestDTO
    {
        public Pagination Pagination { get; set; } = new();
    }
}
