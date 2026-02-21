using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IProductController
    {
        //public Task<ActionResult<IEnumerable<GetAllProductsResponse>>> GetAll(GetAllProductsRequest request);
        public Task<ActionResult<IEnumerable<GetAllProductsResponse>>> GetProducts(GetAllProductsRequest request);
        public Task<ActionResult<IEnumerable<GetAllProductsResponse>>> GetProductByName([FromBody] SearchProductRequestDTO request);


    }
}