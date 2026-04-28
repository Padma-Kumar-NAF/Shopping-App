using ProductApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApi.Application.DTOs.Conversions
{
    public static class ProductConversion
    {
        public static Product ToEntity(ProductDTO product) => new()
        {
            ProductId = product.Id,
            Name = product.Name,
            Quantity = product.Quantity,
            Price = product.Price,
        };


        public static (ProductDTO?, IEnumerable<ProductDTO>?) FromEntity(Product product,IEnumerable<Product>? products)
        {
            if(product is not null || products is null)
            {
                var singleProduct = new ProductDTO(
                    product.ProductId,product.Name,product.Quantity,product.Quantity
                    );
                return (singleProduct,null);
            }

            if(products is not null || product is null)
            {
                var _products = products.Select(p => new ProductDTO(p.ProductId, p.Name, p.Quantity, p.Quantity)).ToList();
                return (null, _products);
            }
            return(null, null);
        }
    }
}
