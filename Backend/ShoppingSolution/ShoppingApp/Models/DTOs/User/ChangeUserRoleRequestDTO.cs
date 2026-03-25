using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.User
{
    public class ChangeUserRoleRequestDTO
    {
        [Required(ErrorMessage = "User id required")]
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public string Role {  get; set; } = string.Empty;
    }
}
