using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IProductService
    {
        public Task<IEnumerable<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request);
        public Task<IEnumerable<GetAllProductsResponseDTO>> SearchProducts(SearchProductRequestDTO request);
        public Task<GetAllProductsResponseDTO> AddProduct(AddNewProductRequestDTO request);
        public Task<UpdateProductResponseDTO> UpdateProduct(UpdateProductRequestDTO request);

    }
}
