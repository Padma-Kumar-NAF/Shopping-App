using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Log
    {
        [Key]
        public Guid LogId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Controller {  get; set; } = string.Empty; // From which controller
        public string Action { get; set; } = string.Empty; // From which method
        public string HttpMethod {  get; set; } = string.Empty;
        public string RequestPath {  get; set; } = string.Empty; // End point
        public Guid? UserId { get; set; }
        public DateTime CreatedAt {  get; set; } 

        // navigation
        public User? User { get; set; }

        //public string ErrorNumber { get; set; } = string.Empty;
    }
}
