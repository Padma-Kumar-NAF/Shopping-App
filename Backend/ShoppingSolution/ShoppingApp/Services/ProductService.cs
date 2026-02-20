using Microsoft.EntityFrameworkCore;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Repositories;
using System;

namespace ShoppingApp.Services
{
    public class ProductService : IProductService
    {
        IRepository<Guid ,Product> _repository;
        public ProductService(IRepository<Guid, Product> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<GetAllProductsResponse>> GetProducts(GetAllProductsRequest request)
        {
            var query = _repository.GetQueryable();

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.Limit <= 0)
                request.Limit = 10;

            if (request.CategoryId != Guid.Empty)
            {
                query = query
                    .Where(p => p.CategoryId == request.CategoryId)
                    .OrderBy(p => p.ProductId);
            }
            else
            {
                var totalCount = await query.CountAsync();

                if (totalCount == 0)
                    return new List<GetAllProductsResponse>();

                var random = new Random();

                int maxSkip = Math.Max(0, totalCount - request.Limit);
                int randomSkip = random.Next(0, maxSkip + 1);

                query = query
                    .OrderBy(p => p.ProductId)
                    .Skip(randomSkip);
            }

            var products = await query
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
                .Skip((request.PageNumber - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync();

            return products;
        }
    }
}