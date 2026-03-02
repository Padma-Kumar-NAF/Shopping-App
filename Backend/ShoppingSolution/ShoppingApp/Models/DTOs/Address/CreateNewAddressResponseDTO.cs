namespace ShoppingApp.Models.DTOs.Address
{
    public class CreateNewAddressResponseDTO
    {
        public Guid AddressId { get; set; }
        public Guid UserId { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
    }
}
