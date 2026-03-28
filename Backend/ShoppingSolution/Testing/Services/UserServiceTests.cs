using Microsoft.EntityFrameworkCore;
using Moq;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.User;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class UserServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private UserService GetService(ShoppingContext context, IPasswordService? passwordService = null, ITokenService? tokenService = null)
        {
            var mockPassword = passwordService ?? CreateDefaultPasswordMock();
            var mockToken = tokenService ?? CreateDefaultTokenMock();

            return new UserService(
                mockPassword,
                new Repository<Guid, User>(context),
                new Repository<Guid, UserDetails>(context),
                new Repository<Guid, Address>(context),
                new UnitOfWork(context),
                mockToken
            );
        }

        private IPasswordService CreateDefaultPasswordMock()
        {
            var mock = new Mock<IPasswordService>();
            mock.Setup(x => x.HashPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="),
                               Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAA==")));
            mock.Setup(x => x.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            return mock.Object;
        }

        private ITokenService CreateDefaultTokenMock()
        {
            var mock = new Mock<ITokenService>();
            mock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("fake-jwt-token");
            return mock.Object;
        }

        private CreateUserRequestDTO ValidCreateRequest(string email = "test@test.com", string phone = "1234567890") =>
            new CreateUserRequestDTO
            {
                Name = "Test User",
                Email = email,
                Password = "password123",
                PhoneNumber = phone,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 1",
                State = "CA",
                City = "LA",
                Pincode = "123456"
            };

        // ── CreateUser ───────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateUser_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.CreateUser(ValidCreateRequest());
            Assert.True(result.Data.IsSuccess);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task CreateUser_DuplicateEmail_Throws409()
        {
            var context = GetDbContext();
            context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Name = "Existing",
                Email = "test@test.com",
                Password = "x",
                SaltValue = "s",
                Role = "user",
                Active = true
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.CreateUser(ValidCreateRequest("test@test.com")));
            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task CreateUser_DuplicatePhone_Throws409()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "other@test.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.UserDetails.Add(new UserDetails
            {
                UserDetailsId = Guid.NewGuid(),
                UserId = userId,
                Name = "U",
                Email = "other@test.com",
                PhoneNumber = "1234567890",
                AddressLine1 = "A",
                AddressLine2 = "B",
                State = "S",
                City = "C",
                Pincode = "123456"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.CreateUser(ValidCreateRequest("new@test.com", "1234567890")));
            Assert.Equal(409, ex.StatusCode);
        }

        // ── LoginUser ────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginUser_Success_ReturnsToken()
        {
            var context = GetDbContext();
            context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Name = "User",
                Email = "login@test.com",
                Password = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
                SaltValue = "AAAAAAAAAAAAAAAAAAAAAA==",
                Role = "user",
                Active = true
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.LoginUser(new LoginRequestDTO { Email = "login@test.com", Password = "password123" });
            Assert.Equal("fake-jwt-token", result.Data.Token);
        }

        [Fact]
        public async Task LoginUser_InvalidEmail_Throws400()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.LoginUser(new LoginRequestDTO { Email = "nobody@test.com", Password = "pass" }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task LoginUser_InactiveUser_ThrowsException()
        {
            var context = GetDbContext();
            context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Name = "Inactive",
                Email = "inactive@test.com",
                Password = "x",
                SaltValue = "s",
                Role = "user",
                Active = false
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.LoginUser(new LoginRequestDTO { Email = "inactive@test.com", Password = "pass" }));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoginUser_WrongPassword_Throws400()
        {
            var context = GetDbContext();
            context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Name = "User",
                Email = "user@test.com",
                Password = "hash",
                SaltValue = "salt",
                Role = "user",
                Active = true
            });
            await context.SaveChangesAsync();

            var mockPassword = new Mock<IPasswordService>();
            mockPassword.Setup(x => x.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var service = GetService(context, mockPassword.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.LoginUser(new LoginRequestDTO { Email = "user@test.com", Password = "wrong" }));
            Assert.Equal(400, ex.StatusCode);
        }

        // ── GetAllUsers ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllUsers_WithData_ReturnsList()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.UserDetails.Add(new UserDetails
            {
                UserDetailsId = Guid.NewGuid(),
                UserId = userId,
                Name = "U",
                Email = "u@u.com",
                PhoneNumber = "1234567890",
                AddressLine1 = "A",
                AddressLine2 = "B",
                State = "S",
                City = "C",
                Pincode = "123456"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetAllUsers(new GetUsersRequestDTO { Pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.NotEmpty(result.Data.UsersList);
        }

        [Fact]
        public async Task GetAllUsers_Empty_ReturnsNoUsersMessage()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var result = await service.GetAllUsers(new GetUsersRequestDTO { Pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.Equal("No users found", result.Message);
        }

        [Fact]
        public async Task GetAllUsers_InvalidPagination_DefaultsToPage1()
        {
            var context = GetDbContext();
            var service = GetService(context);
            // Should not throw, just default to page 1
            var result = await service.GetAllUsers(new GetUsersRequestDTO { Pagination = new Pagination { PageNumber = -1, PageSize = -5 } });
            Assert.Equal(200, result.StatusCode);
        }

        // ── GetUserById ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetUserById_Found_ReturnsUser()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.UserDetails.Add(new UserDetails
            {
                UserDetailsId = Guid.NewGuid(),
                UserId = userId,
                Name = "U",
                Email = "u@u.com",
                PhoneNumber = "1234567890",
                AddressLine1 = "A",
                AddressLine2 = "B",
                State = "S",
                City = "C",
                Pincode = "123456"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetUserById(userId);
            Assert.Equal(userId, result.Data.UserId);
        }

        [Fact]
        public async Task GetUserById_NotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.GetUserById(Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetUserById_NoUserDetails_Throws404()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.GetUserById(userId));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── EditUserEmail ────────────────────────────────────────────────────────

        [Fact]
        public async Task EditUserEmail_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "old@test.com", Password = "hash", SaltValue = "salt", Role = "user", Active = true });
            context.UserDetails.Add(new UserDetails
            {
                UserDetailsId = Guid.NewGuid(),
                UserId = userId,
                Name = "U",
                Email = "old@test.com",
                PhoneNumber = "1234567890",
                AddressLine1 = "A",
                AddressLine2 = "B",
                State = "S",
                City = "C",
                Pincode = "123456"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.EditUserEmail(userId, new EditUserEmailRequestDTO
            {
                OldEmail = "old@test.com",
                NewEmail = "new@test.com",
                Password = "password"
            });
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task EditUserEmail_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserEmail(Guid.NewGuid(), new EditUserEmailRequestDTO
                {
                    OldEmail = "x@x.com",
                    NewEmail = "y@y.com",
                    Password = "pass"
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task EditUserEmail_NewEmailAlreadyExists_Throws400()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "old@test.com", Password = "hash", SaltValue = "salt", Role = "user", Active = true });
            context.Users.Add(new User { UserId = Guid.NewGuid(), Name = "U2", Email = "taken@test.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserEmail(userId, new EditUserEmailRequestDTO
                {
                    OldEmail = "old@test.com",
                    NewEmail = "taken@test.com",
                    Password = "pass"
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task EditUserEmail_WrongPassword_Throws401()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "old@test.com", Password = "hash", SaltValue = "salt", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var mockPassword = new Mock<IPasswordService>();
            mockPassword.Setup(x => x.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var service = GetService(context, mockPassword.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditUserEmail(userId, new EditUserEmailRequestDTO
                {
                    OldEmail = "old@test.com",
                    NewEmail = "new@test.com",
                    Password = "wrong"
                }));
            Assert.Equal(401, ex.StatusCode);
        }

        // ── DeactivateUser ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeactivateUser_ActiveUser_DeactivatesAndReturnsSuccess()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.DeactivateUser(Guid.NewGuid(), userId);
            Assert.True(result.Data.UnActivated);
            Assert.Contains("deactivated", result.Message);
        }

        [Fact]
        public async Task DeactivateUser_InactiveUser_ActivatesUser()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = false });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.DeactivateUser(Guid.NewGuid(), userId);
            Assert.Contains("activated", result.Message);
        }

        [Fact]
        public async Task DeactivateUser_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeactivateUser(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── ChangeUserRole ───────────────────────────────────────────────────────

        [Fact]
        public async Task ChangeUserRole_Success_ReturnsIsChanged()
        {
            var context = GetDbContext();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = adminId, Name = "Admin", Email = "admin@test.com", Password = "x", SaltValue = "s", Role = "admin", Active = true });
            context.Users.Add(new User { UserId = userId, Name = "User", Email = "user@test.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.ChangeUserRole(adminId, userId, "admin");
            Assert.True(result.Data.IsChanged);
        }

        [Fact]
        public async Task ChangeUserRole_InvalidRole_Throws400()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ChangeUserRole(Guid.NewGuid(), Guid.NewGuid(), "superuser"));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task ChangeUserRole_SelfRoleChange_Throws401()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var id = Guid.NewGuid();
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ChangeUserRole(id, id, "admin"));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task ChangeUserRole_NonAdminCaller_Throws401()
        {
            var context = GetDbContext();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = adminId, Name = "NotAdmin", Email = "na@test.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.Users.Add(new User { UserId = userId, Name = "User", Email = "u@test.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ChangeUserRole(adminId, userId, "admin"));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task ChangeUserRole_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var adminId = Guid.NewGuid();
            context.Users.Add(new User { UserId = adminId, Name = "Admin", Email = "admin@test.com", Password = "x", SaltValue = "s", Role = "admin", Active = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ChangeUserRole(adminId, Guid.NewGuid(), "user"));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task ChangeUserRole_AlreadyHasRole_ReturnsIsChangedFalse()
        {
            var context = GetDbContext();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = adminId, Name = "Admin", Email = "admin@test.com", Password = "x", SaltValue = "s", Role = "admin", Active = true });
            context.Users.Add(new User { UserId = userId, Name = "User", Email = "user@test.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.ChangeUserRole(adminId, userId, "user");
            Assert.False(result.Data.IsChanged);
        }

        // ── UpdateUserDetails ────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateUserDetails_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "Old", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.UserDetails.Add(new UserDetails
            {
                UserDetailsId = Guid.NewGuid(),
                UserId = userId,
                Name = "Old",
                Email = "u@u.com",
                PhoneNumber = "1234567890",
                AddressLine1 = "A",
                AddressLine2 = "B",
                State = "S",
                City = "C",
                Pincode = "123456"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.UpdateUserDetails(userId, new UpdateProfileRequestDTO
            {
                Details = new UpdateUserDetailsDTO
                {
                    Name = "New Name",
                    PhoneNumber = "9876543210",
                    AddressLine1 = "New A",
                    AddressLine2 = "New B",
                    State = "NY",
                    City = "NYC",
                    Pincode = "654321"
                }
            });
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task UpdateUserDetails_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateUserDetails(Guid.NewGuid(), new UpdateProfileRequestDTO
                {
                    Details = new UpdateUserDetailsDTO { Name = "X", PhoneNumber = "1234567890", AddressLine1 = "A", AddressLine2 = "B", State = "S", City = "C", Pincode = "123456" }
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateUserDetails_CreatesAddressWhenNoneExists()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.UserDetails.Add(new UserDetails
            {
                UserDetailsId = Guid.NewGuid(),
                UserId = userId,
                Name = "U",
                Email = "u@u.com",
                PhoneNumber = "1234567890",
                AddressLine1 = "A",
                AddressLine2 = "B",
                State = "S",
                City = "C",
                Pincode = "123456"
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.UpdateUserDetails(userId, new UpdateProfileRequestDTO
            {
                Details = new UpdateUserDetailsDTO
                {
                    Name = "U",
                    PhoneNumber = "9876543210",
                    AddressLine1 = "New A",
                    AddressLine2 = "New B",
                    State = "NY",
                    City = "NYC",
                    Pincode = "654321"
                }
            });
            Assert.True(result.Data.IsSuccess);
            Assert.Single(context.Addresses.Where(a => a.UserId == userId));
        }
    }
}
