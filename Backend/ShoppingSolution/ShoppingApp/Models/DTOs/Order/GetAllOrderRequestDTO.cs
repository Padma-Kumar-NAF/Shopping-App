namespace ShoppingApp.Models.DTOs.Order
{
    public class GetAllOrderRequestDTO
    {
        public Pagination pagination { get; set; } = new Pagination();
    }
}