namespace ShoppingApp.Models.DTOs.User
{
    public record ChangeUserRoleResponseDTO
    {
        public bool IsChanged { get; set; } = false;
    }
}
