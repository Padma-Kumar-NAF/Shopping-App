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
            var query = _repository.GetQueryable().AsNoTracking();

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.Limit <= 0)
                request.Limit = 10;

            if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
            {
                query = query
                    .Where(p => p.CategoryId == request.CategoryId.Value)
                    .OrderBy(p => p.Name);
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
                    .OrderBy(p => p.Name)
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

        public async Task<IEnumerable<GetAllProductsResponse>> SearchProducts(SearchProductRequestDTO request)
        {
            var result = new List<GetAllProductsResponse>();

            var query = _repository.GetQueryable().AsNoTracking();

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.Limit <= 0)
                request.Limit = 10;

            if (request.ProductId != Guid.Empty)
            {
                var searchedProduct = await query
                    .Where(p => p.ProductId == request.ProductId)
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
                    .FirstOrDefaultAsync();

                if (searchedProduct != null)
                    result.Add(searchedProduct);
            }

            if (request.CategoryId == Guid.Empty)
                return result;

            var categoryRequest = new GetAllProductsRequest
            {
                CategoryId = request.CategoryId,
                PageNumber = request.PageNumber,
                Limit = request.Limit
            };

            var categoryProducts = await GetProducts(categoryRequest);
            

            var filteredCategoryProducts = categoryProducts // -> This is for remove duplicates
                .Where(p => p.ProductId != request.ProductId);

            result.AddRange(filteredCategoryProducts);

            return result;
        }
    }
}