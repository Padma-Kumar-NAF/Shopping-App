namespace ShoppingApp.Models.DTOs.Cart
{
    public class RemoveAllFromCartResponseDTO
    {
        public bool IsRemoved {  get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
