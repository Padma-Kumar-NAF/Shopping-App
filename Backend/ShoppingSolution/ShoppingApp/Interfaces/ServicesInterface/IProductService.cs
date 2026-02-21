using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IProductService
    {
        public Task<IEnumerable<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request);
        public Task<IEnumerable<GetAllProductsResponseDTO>> SearchProducts(SearchProductRequestDTO request);

    }
}
