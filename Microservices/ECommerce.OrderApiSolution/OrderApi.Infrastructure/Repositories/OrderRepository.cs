using ECommerce.SharedLibrary.Logs;
using ECommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using OrderApi.Application.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Infrastructure.Data;
using System.Linq.Expressions;

namespace OrderApi.Infrastructure.Repositories
{
    public class OrderRepository(OrderDbContext context) : IOrder
    {
        public async Task<Response> CreateAsync(Order entity)
        {
            try
            {
                if (entity == null)
                    return new Response(false, "Order cannot be null");

                var currentEntity = context.Add(entity).Entity;
                await context.SaveChangesAsync();

                if (currentEntity != null && currentEntity.Id > 0)
                    return new Response(true, "Order placed successfully");

                return new Response(false, "Error occurred while placing order");
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response(false, "Error occurred while placing order");
            }
        }

        public async Task<Response> DeleteAsync(Order entity)
        {
            try
            {
                if (entity == null)
                    return new Response(false, "Order cannot be null");

                var getOrder = await FindByIdAsync(entity.Id);

                if (getOrder is null)
                    return new Response(false, $"Order with Id {entity.Id} not found");

                context.Orders.Remove(getOrder);
                await context.SaveChangesAsync();

                return new Response(true, "Order deleted successfully");
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response(false, "Error occurred while deleting order");
            }
        }

        public async Task<Order?> FindByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return null;

                var order = await context.Orders.FindAsync(id);
                return order;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return null;
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                var orders = await context.Orders
                                          .AsNoTracking()
                                          .ToListAsync();

                return orders ?? Enumerable.Empty<Order>();
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return Enumerable.Empty<Order>();
            }
        }

        public async Task<Order?> GetByAsync(Expression<Func<Order, bool>> predicate)
        {
            try
            {
                if (predicate == null)
                    return null;

                var order = await context.Orders
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(predicate);

                return order;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return null;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(Expression<Func<Order, bool>> predicate)
        {
            try
            {
                if (predicate == null)
                    return Enumerable.Empty<Order>();

                var orders = await context.Orders
                                          .AsNoTracking()
                                          .Where(predicate)
                                          .ToListAsync();

                return orders ?? Enumerable.Empty<Order>();
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return Enumerable.Empty<Order>();
            }
        }

        public async Task<Response> UpdateAsync(Order entity)
        {
            try
            {
                if (entity == null)
                    return new Response(false, "Order cannot be null");

                if (entity.Id <= 0)
                    return new Response(false, "Invalid Order Id");

                var getOrder = await FindByIdAsync(entity.Id);

                if (getOrder is null)
                    return new Response(false, $"Order with Id {entity.Id} not found");

                context.Entry(getOrder).State = EntityState.Detached;

                context.Orders.Update(entity);
                await context.SaveChangesAsync();

                return new Response(true, "Order updated successfully");
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response(false, "Error occurred while updating order");
            }
        }
    }
}