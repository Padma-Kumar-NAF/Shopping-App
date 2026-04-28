using ECommerce.SharedLibrary.Logs;
using ECommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Infrastructure.Data;
using System.Linq.Expressions;

namespace ProductApi.Infrastructure.Repositories
{
    public class ProductRepository(ProductContext context) : IProduct
    {
        public async Task<Response> CreateAsync(Product entity)
        {
            try
            {
                var getProduct = await GetByAsync(_ => _.Name!.Equals(entity.Name));

                if(getProduct is not null && !String.IsNullOrEmpty(getProduct.Name))
                {
                    return new Response(false,$"{entity.Name} already added");
                }

                var currentEntity = context.Add(entity).Entity;
                await context.SaveChangesAsync();

                if(currentEntity is not null && currentEntity.ProductId > 0)
                {
                    return new Response(true,$"{entity.Name} added");
                }
                return new Response(false,$"Error occured while adding {entity.Name}");
                
            }
            catch(Exception ex)
            {
                LogException.LogExceptions(ex);

                return new Response(false,"Error occured while adding new Product");
            }
        }

        public async Task<Response> DeleteAsync(Product entity)
        {
            try
            {
                var getProduct = await FindByIdAsync(entity.ProductId);

                if (getProduct is null)
                {
                    return new Response(false, $"{entity.Name} not fpund");
                }

                context.Prodcuts.Remove(entity);
                await context.SaveChangesAsync();
                return new Response(true, $"{entity.Name} is deleted successfully");

            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                return new Response(false, $"Error occured while deleting {entity.Name}");
            }
        }

        public async Task<Product> FindByIdAsync(int id)
        {
            try
            {
                var getProduct = await context.Prodcuts.FindAsync(id);

                if (getProduct is null)
                {
                    return null!;
                }
                return getProduct;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                throw new Exception("Error occured while finding Product");
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                var getProduct = await context.Prodcuts.AsNoTracking().ToListAsync();
                if(getProduct is null)
                {
                    return Enumerable.Empty<Product>();
                }
                return getProduct;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                throw new Exception("Error occured while Geting all Products");
            }
        }

        public async Task<Product> GetByAsync(Expression<Func<Product, bool>> predicate)
        {
            try
            {
                var getProduct = await context.Prodcuts.Where(predicate).FirstOrDefaultAsync();

                if (getProduct is null)
                {
                    return null!;
                }

                return getProduct;

            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                throw new Exception("Error occured retrieving product");
            }
        }

        public async Task<Response> UpdateAsync(Product entity)
        {
            try
            {
                var getProduct = await FindByIdAsync(entity.ProductId);

                if (getProduct is null)
                {
                    return new Response(false,$"Product not found with {entity.Name}");
                }

                context.Entry(getProduct).State = EntityState.Detached;
                context.Prodcuts.Update(entity);
                await context.SaveChangesAsync();

                return new Response(true,$"{entity.Name} is updated");

            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                return new Response(false, "Error occured updating product");
            }
        }
    }
}
