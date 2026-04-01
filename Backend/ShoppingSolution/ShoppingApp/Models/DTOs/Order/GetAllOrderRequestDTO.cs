namespace ShoppingApp.Models.DTOs.Order
{
    public record GetAllOrderRequestDTO
    {
        public Pagination pagination { get; set; } = new Pagination();
    }
}