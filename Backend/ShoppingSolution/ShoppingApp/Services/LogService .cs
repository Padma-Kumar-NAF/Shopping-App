using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Logs;

namespace ShoppingApp.Services
{
    public class LogService : ILogService
    {
        private readonly IRepository<Guid, Log> _logRepository;

        public LogService(IRepository<Guid, Log> logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task<ApiResponse<GetLogsResponseDTO>> GetLogs(GetLogsRequestDTO request)
        {
            try
            {
                var query = _logRepository
                    .GetQueryable()
                    .OrderByDescending(l => l.CreatedAt);

                var totalCount = await query.CountAsync();

                var logs = await query
                    .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .Select(l => new ErrorLogDTO
                    {
                        Message = l.Message,
                        InnerException = l.InnerException,
                        UserName = l.UserName,
                        Role = l.Role,
                        Controller = l.Controller,
                        StatusCode = l.StatusCode,
                        CreatedAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToListAsync();

                return new ApiResponse<GetLogsResponseDTO>
                {
                    StatusCode = 200,
                    Message = logs.Any() ? "Logs fetched successfully" : "No logs found",
                    Data = new GetLogsResponseDTO
                    {
                        Items = logs,
                        TotalCount = totalCount
                    },
                    Action = "GetLogs"
                };
            }
            catch (Exception ex)
            {
                throw new AppException("Error while fetching logs", ex, 500);
            }
        }
    }
}