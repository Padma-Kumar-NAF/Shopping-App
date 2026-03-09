namespace ShoppingApp.Models.DTOs.Wishlist
{
    public class GetUserWishListResponseDTO
    {
        public ICollection<WishListDTO>? WishList { get; set; }
    }
    public class WishListDTO
    {
        public Guid WishListId { get; set; }
        public string WishListName { get; set; } = string.Empty;
        public ICollection<WishListItemsDTO>? WishListItems { get; set; }
    }

    public class WishListItemsDTO
    {
        public Guid WishListItemsId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage {  get; set; } = string.Empty;
    }
}
