namespace ShoppingApp.Models.DTOs.Wishlist
{
    public record GetUserWishListResponseDTO
    {
        public ICollection<WishListDTO> WishList { get; set; } = new HashSet<WishListDTO>();
    }
    public record WishListDTO
    {
        public Guid WishListId { get; set; }
        public string WishListName { get; set; } = string.Empty;
        public ICollection<WishListItemsDTO> WishListItems { get; set; } = new List<WishListItemsDTO>();
    }

    public record WishListItemsDTO
    {
        public Guid WishListItemsId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage {  get; set; } = string.Empty;
    }
}