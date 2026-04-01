namespace ShoppingApp.Models.DTOs.Promocode
{
    public record GetAllPromocodeRequestDTO
    {
        public Pagination Pagination { get; set; } = new Pagination();
    }
}
