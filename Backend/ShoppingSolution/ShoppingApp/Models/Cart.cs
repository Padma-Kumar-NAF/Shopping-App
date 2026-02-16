using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Cart
    {
        [Key]
        public Guid CartId {  get; set; }

        [Required]
        public Guid ProductId {  get; set; }

        [Required]
        public Guid UserId{ get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
