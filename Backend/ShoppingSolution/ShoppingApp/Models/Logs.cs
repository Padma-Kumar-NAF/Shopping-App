using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Logs
    {
        [Key]
        public Guid LogId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string ErrorNumber { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required] 
        public Guid UserId { get; set; }

        [Required]
        public DateTime CreatedAt {  get; set; }
    }
}
