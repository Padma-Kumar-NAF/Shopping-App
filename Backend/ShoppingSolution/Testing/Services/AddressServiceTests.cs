using Microsoft.EntityFrameworkCore;
using Moq;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
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
        // -----------------------------------------------------------------------
        // Infrastructure helpers
        // -----------------------------------------------------------------------

        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(
                    Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private AddressService GetService(ShoppingContext context)
            => new AddressService(
                new Repository<Guid, Address>(context),
                new Repository<Guid, User>(context),
                new Repository<Guid, Order>(context));

        private async Task<Guid> SeedUserAsync(ShoppingContext context)
        {
            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                UserId = userId,
                Name = "U",
                Email = "u@u.com",
                Password = "x",
                SaltValue = "s",
                Role = "user",
                Active = true
            });
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

        private GetUserAddressRequestDTO PageRequest(int page = 1, int size = 10) =>
            new GetUserAddressRequestDTO { Pagination = new Pagination { PageNumber = page, PageSize = size } };

        // -----------------------------------------------------------------------
        // AddAddress
        // -----------------------------------------------------------------------

        [Fact]
        public async Task AddAddress_Success_ReturnsAddressId()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var result = await service.AddAddress(userId, ValidAddressRequest());

            Assert.NotEqual(Guid.Empty, result.Data.AddressId);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Address added successfully", result.Message);
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

        [Fact]
        public async Task AddAddress_DuplicateAddressLine1_CaseInsensitive_Throws409()
        {
            // Duplicate detection normalizes via Trim().ToLower()
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest("123 Main St"));

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddAddress(userId, ValidAddressRequest("123 MAIN ST")));

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task AddAddress_SameAddressLine1_DifferentUser_Succeeds()
        {
            // Same AddressLine1 for a different user must not conflict
            var context = GetDbContext();
            var userId1 = await SeedUserAsync(context);
            var userId2 = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId1, ValidAddressRequest("Shared Street"));
            var result = await service.AddAddress(userId2, ValidAddressRequest("Shared Street"));

            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AddAddress_NullReturnedByRepository_Throws500()
        {
            // Covers the "address == null → 500" branch via Moq
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);

            var mockAddressRepo = new Mock<IRepository<Guid, Address>>();
            mockAddressRepo.Setup(r => r.GetQueryable())
                .Returns(context.Addresses.AsQueryable());
            mockAddressRepo.Setup(r => r.AddAsync(It.IsAny<Address>()))
                .ReturnsAsync((Address?)null);

            var service = new AddressService(
                mockAddressRepo.Object,
                new Repository<Guid, User>(context),
                new Repository<Guid, Order>(context));

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddAddress(userId, ValidAddressRequest()));

            Assert.Equal(500, ex.StatusCode);
        }

        // -----------------------------------------------------------------------
        // GetUserAddress
        // -----------------------------------------------------------------------

        [Fact]
        public async Task GetUserAddress_WithData_ReturnsList()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest());
            var result = await service.GetUserAddress(userId, PageRequest());

            Assert.NotEmpty(result.Data.AddressList);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Address fetched successfully", result.Message);
        }

        [Fact]
        public async Task GetUserAddress_Empty_ReturnsNoAddressMessage()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetUserAddress(Guid.NewGuid(), PageRequest());

            Assert.Equal("No address found", result.Message);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data.AddressList);
        }

        [Fact]
        public async Task GetUserAddress_Pagination_ReturnsCorrectPage()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest("Street A"));
            await service.AddAddress(userId, ValidAddressRequest("Street B"));
            await service.AddAddress(userId, ValidAddressRequest("Street C"));

            var page1 = await service.GetUserAddress(userId, PageRequest(1, 2));
            Assert.Equal(2, page1.Data.AddressList.Count);

            var page2 = await service.GetUserAddress(userId, PageRequest(2, 2));
            Assert.Single(page2.Data.AddressList);
        }

        [Fact]
        public async Task GetUserAddress_ReturnsCorrectAddressFields()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, new CreateNewAddressRequestDTO
            {
                AddressLine1 = "10 Elm St",
                AddressLine2 = "Floor 3",
                State = "TX",
                City = "Austin",
                PinCode = "787878"
            });

            var result = await service.GetUserAddress(userId, PageRequest());
            var addr = result.Data.AddressList.First();

            Assert.Equal("10 Elm St", addr.AddressLine1);
            Assert.Equal("Floor 3", addr.AddressLine2);
            Assert.Equal("TX", addr.State);
            Assert.Equal("Austin", addr.City);
            Assert.Equal("787878", addr.Pincode);
        }

        // -----------------------------------------------------------------------
        // DeleteUserAddress
        // -----------------------------------------------------------------------

        [Fact]
        public async Task DeleteUserAddress_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest());
            var result = await service.DeleteUserAddress(userId,
                new DeleteUserAddressRequestDTO { AddressId = added.Data.AddressId });

            Assert.True(result.Data.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Address deleted successfully", result.Message);
        }

        [Fact]
        public async Task DeleteUserAddress_NotFound_Throws404()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteUserAddress(userId,
                    new DeleteUserAddressRequestDTO { AddressId = Guid.NewGuid() }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteUserAddress_AddressBelongsToOtherUser_Throws404()
        {
            var context = GetDbContext();
            var userId1 = await SeedUserAsync(context);
            var userId2 = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId1, ValidAddressRequest());

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteUserAddress(userId2,
                    new DeleteUserAddressRequestDTO { AddressId = added.Data.AddressId }));

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
                service.DeleteUserAddress(userId,
                    new DeleteUserAddressRequestDTO { AddressId = addressId }));

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteUserAddress_NullReturnedByRepository_Throws500()
        {
            // Covers the "deleted == null → 500" branch via Moq
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);

            var address = new Address
            {
                AddressId = Guid.NewGuid(),
                UserId = userId,
                AddressLine1 = "Test St",
                AddressLine2 = "Apt 1",
                State = "CA",
                City = "LA",
                Pincode = "123456"
            };

            var mockAddressRepo = new Mock<IRepository<Guid, Address>>();
            mockAddressRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);
            mockAddressRepo
                .Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Address?)null);

            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();
            mockOrderRepo.Setup(r => r.GetQueryable())
                .Returns(context.Orders.AsQueryable());

            var service = new AddressService(
                mockAddressRepo.Object,
                new Repository<Guid, User>(context),
                mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteUserAddress(userId,
                    new DeleteUserAddressRequestDTO { AddressId = address.AddressId }));

            Assert.Equal(500, ex.StatusCode);
        }

        // -----------------------------------------------------------------------
        // EditUserAddress
        // -----------------------------------------------------------------------

        [Fact]
        public async Task EditUserAddress_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest("Old Street"));

            var result = await service.EditUserAddress(userId, new EditUserAddressRequestDTO
            {
                AddressId = added.Data.AddressId,
                AddressLine1 = "New Street",
                AddressLine2 = "Suite 2",
                State = "NY",
                City = "NYC",
                Pincode = "654321"
            });

            Assert.True(result.Data.IsSuccess);
            Assert.Equal(200, result.StatusCode);
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
        public async Task EditUserAddress_AddressBelongsToOtherUser_Throws404()
        {
            var context = GetDbContext();
            var userId1 = await SeedUserAsync(context);
            var userId2 = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId1, ValidAddressRequest());

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId2, new EditUserAddressRequestDTO
                {
                    AddressId = added.Data.AddressId,
                    AddressLine1 = "New",
                    AddressLine2 = "B",
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
        public async Task EditUserAddress_NoChanges_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest("Same Street"));

            var result = await service.EditUserAddress(userId, new EditUserAddressRequestDTO
            {
                AddressId = added.Data.AddressId,
                AddressLine1 = "Same Street",
                AddressLine2 = "Apt 1",
                State = "CA",
                City = "LA",
                Pincode = "123456"
            });

            Assert.True(result.Data.IsSuccess);
            Assert.Equal("NoChangesRequired", result.Action);
        }

        [Fact]
        public async Task EditUserAddress_NoChanges_WhitespaceTrimmed_ReturnsNoChange()
        {
            // Values with surrounding whitespace that normalize to the same → treated as no change
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest("Same Street"));

            var result = await service.EditUserAddress(userId, new EditUserAddressRequestDTO
            {
                AddressId = added.Data.AddressId,
                AddressLine1 = "  Same Street  ",
                AddressLine2 = "  Apt 1  ",
                State = "  CA  ",
                City = "  LA  ",
                Pincode = "  123456  "
            });

            Assert.True(result.Data.IsSuccess);
            Assert.Equal("NoChangesRequired", result.Action);
        }

        [Fact]
        public async Task EditUserAddress_OnlyOneFieldChanged_TriggersUpdate()
        {
            // Only City changes — isChanged should be true and update should succeed
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            var added = await service.AddAddress(userId, ValidAddressRequest("Old Street"));

            var result = await service.EditUserAddress(userId, new EditUserAddressRequestDTO
            {
                AddressId = added.Data.AddressId,
                AddressLine1 = "Old Street",
                AddressLine2 = "Apt 1",
                State = "CA",
                City = "San Francisco",   // only this field changed
                Pincode = "123456"
            });

            Assert.True(result.Data.IsSuccess);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task EditUserAddress_DuplicateLine1_Throws409()
        {
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest("First Street"));
            var second = await service.AddAddress(userId, ValidAddressRequest("Second Street"));

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId, new EditUserAddressRequestDTO
                {
                    AddressId = second.Data.AddressId,
                    AddressLine1 = "First Street",   // conflicts with existing address
                    AddressLine2 = "B",
                    State = "S",
                    City = "C",
                    Pincode = "123456"
                }));

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task EditUserAddress_DuplicateLine1_CaseInsensitive_Throws409()
        {
            // Duplicate check is case-insensitive (Trim().ToLower())
            var context = GetDbContext();
            var userId = await SeedUserAsync(context);
            var service = GetService(context);

            await service.AddAddress(userId, ValidAddressRequest("First Street"));
            var second = await service.AddAddress(userId, ValidAddressRequest("Second Street"));

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId, new EditUserAddressRequestDTO
                {
                    AddressId = second.Data.AddressId,
                    AddressLine1 = "FIRST STREET",   // same after ToLower()
                    AddressLine2 = "B",
                    State = "S",
                    City = "C",
                    Pincode = "123456"
                }));

            Assert.Equal(409, ex.StatusCode);
        }
    }
}
