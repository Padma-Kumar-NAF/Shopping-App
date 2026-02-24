using Microsoft.EntityFrameworkCore;
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
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
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
    }
}