using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IProductService
    {
        public Task<IEnumerable<GetAllProductsResponse>> GetProducts(GetAllProductsRequest request);
        public Task<IEnumerable<GetAllProductsResponse>> SearchProducts(SearchProductRequestDTO request);

    }
}
