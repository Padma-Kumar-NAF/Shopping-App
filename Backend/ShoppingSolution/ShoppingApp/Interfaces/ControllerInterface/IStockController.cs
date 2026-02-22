using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IStockController
    {
        public Task<ActionResult<IEnumerable<GetStockResponseDTO>>> GetStock(GetStockRequestDTO request);
    }
}
