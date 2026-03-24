namespace ShoppingApp.Models.DTOs.Logs
{
    public class GetLogsResponseDTO
    {
        public List<ErrorLogDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class ErrorLogDTO
    {
        public string Message { get; set; } = string.Empty;
        public string InnerException { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public int StatusCode { get; set; } 
        public string CreatedAt { get; set; } = string.Empty;

    }
}
