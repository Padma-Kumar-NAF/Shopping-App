using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Logs;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ILogService
    {
        Task<ApiResponse<GetLogsResponseDTO>> GetLogs(GetLogsRequestDTO request);
    }
}
