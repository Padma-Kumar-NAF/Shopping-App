using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IProductService
    {
        public Task<IEnumerable<GetAllProductsResponse>> GetAllProducts();
    }
}
