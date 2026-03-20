using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IProductController
    {
        public Task<IActionResult> GetProducts(GetAllProductsRequestDTO request);
        public Task<IActionResult> GetProductByName(SearchProductRequestDTO request);
        public Task<IActionResult> GetProductById(SearchProductByIdRequestDTO request);
        public Task<IActionResult> AddProduct(AddNewProductRequestDTO request);
        public Task<IActionResult> UpdateProduct(UpdateProductRequestDTO request);
    }
}