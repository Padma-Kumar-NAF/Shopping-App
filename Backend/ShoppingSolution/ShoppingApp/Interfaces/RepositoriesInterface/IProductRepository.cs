using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface IProductRepository
    {
        public Task<IEnumerable<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request);
        public Task<IEnumerable<GetAllProductsResponseDTO>> SearchProducts(SearchProductRequestDTO request);
        public Task<GetAllProductsResponseDTO> AddProduct(AddNewProductRequestDTO request);
    }
}