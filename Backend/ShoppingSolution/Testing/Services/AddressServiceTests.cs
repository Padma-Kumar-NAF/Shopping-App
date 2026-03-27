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
using System.Linq.Expressions;
using Xunit;

namespace Testing.Services
{
    public class AddressServiceFullCoverageTests
    {
        // ---------------- COMMON SETUP ----------------

        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
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

        // ---------------- ADD (Extra Coverage) ----------------

        [Fact]
        public async Task AddAddress_GenericException_ShouldThrow500()
        {
            var mockRepo = new Mock<IRepository<Guid, Address>>();
            var mockUserRepo = new Mock<IRepository<Guid, User>>();
            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();

            mockUserRepo.Setup(x => x.GetQueryable())
                .Throws(new Exception("Unexpected"));

            var service = new AddressService(mockRepo.Object, mockUserRepo.Object, mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddAddress(Guid.NewGuid(), new CreateNewAddressRequestDTO()));

            Assert.Equal(500, ex.StatusCode);
        }

        [Fact]
        public async Task AddAddress_UserNotFound_ShouldThrow404()
        {
            var context = GetDbContext(); // no user added
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddAddress(Guid.NewGuid(), new CreateNewAddressRequestDTO()));

            Assert.Equal(404, ex.StatusCode);
        }

        // ---------------- DELETE (Extra Coverage) ----------------

        [Fact]
        public async Task DeleteAddress_GenericException_ShouldThrow500()
        {
            var mockRepo = new Mock<IRepository<Guid, Address>>();
            var mockUserRepo = new Mock<IRepository<Guid, User>>();
            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();

            mockRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var service = new AddressService(mockRepo.Object, mockUserRepo.Object, mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteUserAddress(Guid.NewGuid(),
                    new DeleteUserAddressRequestDTO { AddressId = Guid.NewGuid() }));

            Assert.Equal(500, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteAddress_AddressNull_ShouldThrow404()
        {
            var mockRepo = new Mock<IRepository<Guid, Address>>();
            var mockUserRepo = new Mock<IRepository<Guid, User>>();
            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();

            mockRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);

            var service = new AddressService(mockRepo.Object, mockUserRepo.Object, mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteUserAddress(Guid.NewGuid(),
                    new DeleteUserAddressRequestDTO { AddressId = Guid.NewGuid() }));

            Assert.Equal(404, ex.StatusCode);
        }

        // ---------------- EDIT (Full Branch Coverage) ----------------

        [Fact]
        public async Task EditAddress_NotFound_ShouldThrow404()
        {
            var service = GetService(GetDbContext());

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(Guid.NewGuid(),
                    new EditUserAddressRequestDTO { AddressId = Guid.NewGuid() }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task EditAddress_ShouldHitUpdateBlock()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            context.Addresses.Add(new Address
            {
                AddressId = addressId,
                UserId = userId,
                AddressLine1 = "Old"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.EditUserAddress(userId,
                new EditUserAddressRequestDTO
                {
                    AddressId = addressId,
                    AddressLine1 = "New"
                });

            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task EditAddress_DbUpdateException_ShouldThrow500()
        {
            var mockRepo = new Mock<IRepository<Guid, Address>>();
            var mockUserRepo = new Mock<IRepository<Guid, User>>();
            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();

            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            mockRepo.Setup(x => x.GetQueryable())
                .Returns(new List<Address>
                {
            new Address { AddressId = addressId, UserId = userId, AddressLine1 = "Old" }
                }.AsQueryable());

            mockOrderRepo.Setup(x => x.GetQueryable())
                .Returns(new List<Order>().AsQueryable());

            mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Address>()))
                .ThrowsAsync(new DbUpdateException());

            var service = new AddressService(mockRepo.Object, mockUserRepo.Object, mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId,
                    new EditUserAddressRequestDTO { AddressId = addressId, AddressLine1 = "New" }));

            Assert.Equal(500, ex.StatusCode);
        }

        [Fact]
        public async Task EditAddress_UsedInOrder_ShouldThrow409()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            context.Addresses.Add(new Address
            {
                AddressId = addressId,
                UserId = userId,
                AddressLine1 = "Same"
            });

            context.Orders.Add(new Order { AddressId = addressId });
            await context.SaveChangesAsync();

            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId,
                    new EditUserAddressRequestDTO
                    {
                        AddressId = addressId,
                        AddressLine1 = "New"
                    }));

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task EditAddress_NoChanges_ShouldReturnEarly()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            context.Addresses.Add(new Address
            {
                AddressId = addressId,
                UserId = userId,
                AddressLine1 = "Same"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.EditUserAddress(userId,
                new EditUserAddressRequestDTO
                {
                    AddressId = addressId,
                    AddressLine1 = "Same"
                });

            Assert.Equal("NoChangesRequired", result.Action);
        }

        [Fact]
        public async Task EditAddress_Duplicate_ShouldThrow409()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();

            var address1 = new Address
            {
                AddressId = Guid.NewGuid(),
                UserId = userId,
                AddressLine1 = "Home"
            };

            var address2 = new Address
            {
                AddressId = Guid.NewGuid(),
                UserId = userId,
                AddressLine1 = "Office"
            };

            context.Addresses.AddRange(address1, address2);
            await context.SaveChangesAsync();

            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(userId,
                    new EditUserAddressRequestDTO
                    {
                        AddressId = address2.AddressId,
                        AddressLine1 = "HOME"
                    }));

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task EditAddress_GenericException_ShouldThrow500()
        {
            var mockRepo = new Mock<IRepository<Guid, Address>>();
            var mockUserRepo = new Mock<IRepository<Guid, User>>();
            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();

            mockRepo.Setup(x => x.GetQueryable())
                .Throws(new Exception("Unexpected"));

            var service = new AddressService(mockRepo.Object, mockUserRepo.Object, mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserAddress(Guid.NewGuid(),
                    new EditUserAddressRequestDTO()));

            Assert.Equal(500, ex.StatusCode);
        }

        // ---------------- GET (Full Coverage) ----------------

        [Fact]
        public async Task GetUserAddress_WithData_ShouldReturnList()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();

            context.Addresses.Add(new Address
            {
                AddressId = Guid.NewGuid(),
                UserId = userId,
                AddressLine1 = "Test"
            });

            await context.SaveChangesAsync();

            var service = GetService(context);

            var result = await service.GetUserAddress(userId,
                new GetUserAddressRequestDTO
                {
                    Pagination = new Pagination { PageNumber = 1, PageSize = 10 }
                });

            Assert.Single(result.Data.AddressList);
        }

        [Fact]
        public async Task GetUserAddress_DbUpdateException_ShouldThrow500()
        {
            var mockRepo = new Mock<IRepository<Guid, Address>>();
            var mockUserRepo = new Mock<IRepository<Guid, User>>();
            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();

            mockRepo.Setup(x => x.GetQueryable())
                .Throws(new DbUpdateException());

            var service = new AddressService(mockRepo.Object, mockUserRepo.Object, mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetUserAddress(Guid.NewGuid(),
                    new GetUserAddressRequestDTO
                    {
                        Pagination = new Pagination { PageNumber = 1, PageSize = 10 }
                    }));

            Assert.Equal(500, ex.StatusCode);
        }

        [Fact]
        public async Task GetUserAddress_NoAddress_ShouldReturnEmptyList()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetUserAddress(Guid.NewGuid(),
                new GetUserAddressRequestDTO
                {
                    Pagination = new Pagination { PageNumber = 1, PageSize = 10 }
                });

            Assert.Empty(result.Data.AddressList);
            Assert.Equal("Show address button", result.Action);
        }

        [Fact]
        public async Task GetUserAddress_GenericException_ShouldThrow500()
        {
            var mockRepo = new Mock<IRepository<Guid, Address>>();
            var mockUserRepo = new Mock<IRepository<Guid, User>>();
            var mockOrderRepo = new Mock<IRepository<Guid, Order>>();

            mockRepo.Setup(x => x.GetQueryable())
                .Throws(new Exception("Unexpected"));

            var service = new AddressService(mockRepo.Object, mockUserRepo.Object, mockOrderRepo.Object);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetUserAddress(Guid.NewGuid(),
                    new GetUserAddressRequestDTO
                    {
                        Pagination = new Pagination { PageNumber = 1, PageSize = 10 }
                    }));

            Assert.Equal(500, ex.StatusCode);
        }
    }
}