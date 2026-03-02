using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IStockController
    {
        public Task<ActionResult<IEnumerable<GetStockResponseDTO>>> GetStock(GetStockRequestDTO request);
        public Task<ActionResult<AddNewStockResponseDTO>> AddStock(AddNewStockRequestDTO request);
        //public Task<ActionResult<IEnumerable<GetStockResponseDTO>>> UpdateStock(GetStockRequestDTO request);
        //public Task<ActionResult<IEnumerable<GetStockResponseDTO>>> DeleteStock(GetStockRequestDTO request);
    }
}
