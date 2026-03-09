namespace ShoppingApp.Models
{
    public class WishListItems
    {
        public Guid WishListItemsId { get; set; }
        public Guid WishListId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation 
        public WishList? WishList { get; set; }
        public Product? Products { get; set; }
    }
}
