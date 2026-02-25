using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;
using ShoppingApp.Repositories;
using System;

namespace ShoppingApp.Services
{
    public class ProductService : IProductService
    {
        IProductRepository _productRepository;
        private readonly ShoppingContext _context;
        public ProductService(IProductRepository productRepository, ShoppingContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<GetAllProductsResponseDTO> AddProduct(AddNewProductRequestDTO request)
        {
            try
            {
                var addedProduct = await _productRepository.AddProduct(request);
                return addedProduct;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request)
        {
            try
            {
                var productList = await _productRepository.GetProducts(request);
                return productList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // This is wrong , get the name from user and return the results
        public async Task<IEnumerable<GetAllProductsResponseDTO>> SearchProducts(SearchProductRequestDTO request)
        {

            try
            {
                var searchedProduct = await _productRepository.SearchProducts(request);
                return searchedProduct;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UpdateProductResponseDTO> UpdateProduct(UpdateProductRequestDTO request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

            if (product == null)
                throw new Exception("Product not found");

            var stock = await _context.Stock
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId);

            if (stock == null)
                throw new Exception("Stock not found");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

            if (category == null)
                throw new Exception("Category not found");

            product.CategoryId = request.CategoryId;
            product.Name = request.Name;
            product.ImagePath = request.ImagePath;
            product.Description = request.Description;
            product.Price = request.Price;

            stock.Quantity = request.Quantity;

            await _context.SaveChangesAsync();

            return new UpdateProductResponseDTO
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                StockId = stock.StockId,
                Name = product.Name,
                ImagePath = product.ImagePath,
                Description = product.Description,
                CategoryName = category.CategoryName,
                Price = product.Price,
                Quantity = stock.Quantity
            };
        }
    }
}