namespace ShoppingApp.Models
{
    public class WishList
    {
        public Guid WishListId { get; set; }
        public Guid UserId { get; set; }
        public string WhishListName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // navigation 
        public User? User { get; set; }
        public ICollection<WishListItems>? WishListItems { get; set; } = new List<WishListItems>();
    }
}
