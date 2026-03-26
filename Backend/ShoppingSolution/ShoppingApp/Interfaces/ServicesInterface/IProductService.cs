using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IProductService
    {
        public Task<ApiResponse<AddNewProductResponseDTO>> AddProduct(Guid userId,AddNewProductRequestDTO request);
        public Task<ApiResponse<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request);
        public Task<ApiResponse<GetAllProductsWithFilterResponseDTO>> GetProductsWithFilter(GetAllProductsWithFilterRequestDTO request);
        public Task<ApiResponse<SearchProductByIdResponseDTO>> SearchProductById(SearchProductByIdRequestDTO request);
        public Task<ApiResponse<SearchProductByNameResponseDTO>> SearchProductByName(SearchProductByNameRequestDTO request);
        public Task<ApiResponse<UpdateProductResponseDTO>> UpdateProduct(Guid userId,UpdateProductRequestDTO request);
        public Task<ApiResponse<List<ProductSuggestionDTO>>> GetSuggestions(string query);
        public Task<ApiResponse<DeleteProductResponseDTO>> DeleteProduct(Guid userId, DeleteProductRequestDTO request);
    }
}