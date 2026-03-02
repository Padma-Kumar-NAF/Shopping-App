using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Order
{
    public class GetUserOrderDetailsResponseDTO
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalProductsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public AddressDTO Address { get; set; } = null!;
        public ICollection<OrderDetailsDTO> Items { get; set; } = new List<OrderDetailsDTO>();
    }

    public class AddressDTO
    {
        public Guid AddressId { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
    }

    public class OrderDetailsDTO
    {
        public Guid OrderDetailsId { get; set; }
        public Guid ProductId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
