using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Repositories;

namespace ShoppingApp.Services
{
    public class ProductService : IProductService
    {
        IRepository<Guid ,Product> _repository;
        public ProductService(IRepository<Guid, Product> repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<GetAllProductsResponse>> GetAllProducts()
        {
            var products = await _repository
                .GetQueryable()
                .Select(p => new GetAllProductsResponse
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    CategoryName = p.Category!.CategoryName,
                    Price = p.Price
                })
                .ToListAsync();

            if (!products.Any())
                throw new Exception("Products not found");
            Console.WriteLine("---------------------------");
            Console.WriteLine(products.Count());
            Console.WriteLine("---------------------------");
            return products;
        }
    }
}