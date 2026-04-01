using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Logs;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("logs")]
    [ApiController]
    public class LogsController : BaseController
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpPost("get-logs")]
        public async Task<IActionResult> GetLogs([FromBody] GetLogsRequestDTO request)
        {
            try
            {
                var result = await _logService.GetLogs(request);
                return StatusCode(result.StatusCode, result);
            }
            catch
            {
                throw;
            }
        }
    }
}