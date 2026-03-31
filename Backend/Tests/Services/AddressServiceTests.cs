using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Address;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class AddressServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private AddressService GetService(ShoppingContext context)
        {
            return new AddressService(
                new Repository<Guid, Address>(context),
                new Repository<Guid, User>(context),
                new Repository<Guid, Order>(context)
            );
        }

        private async Task<Guid> SeedUserAsync(ShoppingContext context)
        {
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();
            return userId;
        }

        private CreateNewAddressRequestDTO ValidAddressRequest(string line1 = "123 Main St") =>
            new CreateNewAddressRequestDTO
            {
                AddressLine1 = line1,
                AddressLine2 = "Apt 1",
                State = "CA",
                City = "LA",
                PinCode = "123456"
            };

        // -- AddAddress -----------------------------------------------------------

        [Fact]
        public async Task AddAddress_Success_ReturnsAddressId()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var result = await service.AddAddress(userId, ValidAddressRequest());
            Assert.NotEqual(Guid.Empty, result.Data.AddressId);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AddAddress_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddAddress(Guid.NewGuid(), ValidAddressRequest()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddAddress_DuplicateAddressLine1_Throws409()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest("123 Main St"));
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddAddress(userId, ValidAddressRequest("123 Main St")));
            Assert.Equal(409, ex.StatusCode);
        }

        // -- GetUserAddress -------------------------------------------------------

        [Fact]
        public async Task GetUserAddress_WithData_ReturnsList()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest());
            var result = await service.GetUserAddress(userId, new GetUserAddressRequestDTO { Pagination = new Pagination { PageNumber = 1, PageSize = 10 } });

            Assert.NotEmpty(result.Data.AddressList);
        }

        [Fact]
        public async Task GetUserAddress_Empty_ReturnsNoAddressMessage()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetUserAddress(Guid.NewGuid(), new GetUserAddressRequestDTO { Pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.Equal("No address found", result.Message);
        }

        // -- DeleteUserAddress ----------------------------------------------------

        [Fact]
        public async Task DeleteUserAddress_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest());
            var addressId = added.Data.AddressId;

            var result = await service.DeleteUserAddress(userId, new DeleteUserAddressRequestDTO { AddressId = addressId });
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task DeleteUserAddress_NotFound_Throws404()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteUserAddress(userId, new DeleteUserAddressRequestDTO { AddressId = Guid.NewGuid() }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteUserAddress_UsedInOrder_Throws409()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest());
            var addressId = added.Data.AddressId;

            // Attach address to an order
            context.Orders.Add(new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = userId,
                Status = "Not Delivered",
                TotalProductsCount = 1,
                TotalAmount = 100,
                OrderTotalAmount = 100,
                AddressId = addressId,
                DeliveryDate = DateTime.UtcNow.AddDays(2)
            });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteUserAddress(userId, new DeleteUserAddressRequestDTO { AddressId = addressId }));
            Assert.Equal(409, ex.StatusCode);
        }

        // -- EditUserAddress ------------------------------------------------------

        [Fact]
        public async Task EditUserAddress_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest("Old Street"));
            var addressId = added.Data.AddressId;

            var result = await service.EditUserAddress(userId, new EditUserAddressRequestDTO
            {
                AddressId = addressId,
                AddressLine1 = "New Street",
                AddressLine2 = "Suite 2",
                State = "NY",
                City = "NYC",
                Pincode = "654321"
            });

            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task EditUserAddress_NotFound_Throws404()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId, new EditUserAddressRequestDTO
                {
                    AddressId = Guid.NewGuid(),
                    AddressLine1 = "X",
                    AddressLine2 = "Y",
                    State = "S",
                    City = "C",
                    Pincode = "123456"
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task EditUserAddress_UsedInOrder_Throws409()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest());
            var addressId = added.Data.AddressId;

            context.Orders.Add(new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = userId,
                Status = "Not Delivered",
                TotalProductsCount = 1,
                TotalAmount = 100,
                OrderTotalAmount = 100,
                AddressId = addressId,
                DeliveryDate = DateTime.UtcNow.AddDays(2)
            });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId, new EditUserAddressRequestDTO
                {
                    AddressId = addressId,
                    AddressLine1 = "New",
                    AddressLine2 = "B",
                    State = "S",
                    City = "C",
                    Pincode = "123456"
                }));
            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task EditUserAddress_DuplicateLine1_Throws409()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest("First Street"));
            var second = await service.AddAddress(userId, ValidAddressRequest("Second Street"));
            var secondId = second.Data.AddressId;

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId, new EditUserAddressRequestDTO
                {
                    AddressId = secondId,
                    AddressLine1 = "First Street",
                    AddressLine2 = "B",
                    State = "S",
                    City = "C",
                    Pincode = "123456"
                }));
            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task EditUserAddress_NoChanges_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest("Same Street"));
            var addressId = added.Data.AddressId;

            var result = await service.EditUserAddress(userId, new EditUserAddressRequestDTO
            {
                AddressId = addressId,
                AddressLine1 = "Same Street",
                AddressLine2 = "Apt 1",
                State = "CA",
                City = "LA",
                Pincode = "123456"
            });

            Assert.True(result.Data.IsSuccess);
        }
    }
}
