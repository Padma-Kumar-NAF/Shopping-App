namespace ShoppingApp.Models
{
    public record ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string Action { get; set; } = string.Empty;
    }
}