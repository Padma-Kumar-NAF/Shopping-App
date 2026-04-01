using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUserService
    {
        public Task<ApiResponse<CreateUserResponseDTO>> CreateUser(CreateUserRequestDTO request);
        public Task<ApiResponse<ChangeUserRoleResponseDTO>> ChangeUserRole(Guid AdminId,Guid userId,string role);
        public Task<ApiResponse<EditUserEmailResponseDTO>> EditUserEmail(Guid userId, EditUserEmailRequestDTO request);
        public Task<ApiResponse<DeleteUserResponseDTO>> DeactivateUser(Guid UserId,Guid DeleteUserId);
        public Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginRequestDTO request);
        public Task<ApiResponse<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request);

        /// <summary>
        /// Asynchronously retrieves the details of a user identified by the specified unique identifier.
        /// </summary>
        /// <remarks>Throws an exception if the user ID is invalid or if an error occurs while accessing
        /// the data source.</remarks>
        /// <param name="UserId">The unique identifier of the user to retrieve. This parameter must not be an empty GUID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an ApiResponse with the user
        /// details in a GetUserByIdResponseDTO object if the user is found; otherwise, an error response.</returns>
        public Task<ApiResponse<GetUserByIdResponseDTO>> GetUserById(Guid UserId);
        public Task<ApiResponse<UpdateProfileResponseDTO>> UpdateUserDetails(Guid userId,UpdateProfileRequestDTO request);

    }
}
