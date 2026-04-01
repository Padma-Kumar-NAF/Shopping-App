using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Promocode;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class PromoCodeServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private PromoCodeService GetService(ShoppingContext context)
        {
            return new PromoCodeService(
                new Repository<Guid, PromoCode>(context),
                new UnitOfWork(context)
            );
        }

        private AddPromoCodeRequestDTO ValidRequest(string name = "SAVE10") =>
            new AddPromoCodeRequestDTO
            {
                PromoCodeName = name,
                DiscountPercentage = 10,
                FromDate = DateTime.UtcNow.AddDays(-1),
                ToDate = DateTime.UtcNow.AddDays(30)
            };

        // ── AddPromoCode ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddPromoCode_Success_ReturnsPromoCodeId()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.AddPromoCode(ValidRequest());
            Assert.NotEqual(Guid.Empty, result.Data.PromoCodeId);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AddPromoCode_FromDateGreaterThanToDate_Throws400()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddPromoCode(new AddPromoCodeRequestDTO
                {
                    PromoCodeName = "BAD",
                    DiscountPercentage = 10,
                    FromDate = DateTime.UtcNow.AddDays(5),
                    ToDate = DateTime.UtcNow.AddDays(1)
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task AddPromoCode_Duplicate_Throws401()
        {
            var context = GetDbContext();
            var service = GetService(context);

            await service.AddPromoCode(ValidRequest("DUPE"));
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddPromoCode(ValidRequest("DUPE")));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task AddPromoCode_StoresNameUpperCase()
        {
            var context = GetDbContext();
            var service = GetService(context);

            await service.AddPromoCode(new AddPromoCodeRequestDTO
            {
                PromoCodeName = "lowercase",
                DiscountPercentage = 5,
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow.AddDays(10)
            });

            var promo = context.PromoCodes.First();
            Assert.Equal("LOWERCASE", promo.PromoCodeName);
        }

        // ── EditPromoCode ────────────────────────────────────────────────────────

        [Fact]
        public async Task EditPromoCode_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var added = await service.AddPromoCode(ValidRequest("EDIT10"));
            var promoId = context.PromoCodes.First().PromoCodeId;

            var result = await service.EditPromoCode(new EditPromocodeRequestDTO
            {
                PromoCodeId = promoId,
                PromoCodeName = "EDIT20",
                DiscountPercentage = 20,
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow.AddDays(10)
            });

            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task EditPromoCode_NotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditPromoCode(new EditPromocodeRequestDTO
                {
                    PromoCodeId = Guid.NewGuid(),
                    PromoCodeName = "X",
                    DiscountPercentage = 10,
                    FromDate = DateTime.UtcNow,
                    ToDate = DateTime.UtcNow.AddDays(5)
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task EditPromoCode_FromDateGreaterThanToDate_Throws400()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditPromoCode(new EditPromocodeRequestDTO
                {
                    PromoCodeId = Guid.NewGuid(),
                    PromoCodeName = "X",
                    DiscountPercentage = 10,
                    FromDate = DateTime.UtcNow.AddDays(10),
                    ToDate = DateTime.UtcNow
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task EditPromoCode_DuplicateName_Throws400()
        {
            var context = GetDbContext();
            var service = GetService(context);

            await service.AddPromoCode(ValidRequest("FIRST"));
            await service.AddPromoCode(ValidRequest("SECOND"));
            var secondId = context.PromoCodes.First(p => p.PromoCodeName == "SECOND").PromoCodeId;

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditPromoCode(new EditPromocodeRequestDTO
                {
                    PromoCodeId = secondId,
                    PromoCodeName = "FIRST",
                    DiscountPercentage = 10,
                    FromDate = DateTime.UtcNow,
                    ToDate = DateTime.UtcNow.AddDays(5)
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        // ── DeletePromoCode ──────────────────────────────────────────────────────

        [Fact]
        public async Task DeletePromoCode_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var service = GetService(context);

            await service.AddPromoCode(ValidRequest("DEL10"));
            var promoId = context.PromoCodes.First().PromoCodeId;

            var result = await service.DeletePromoCode(new DeletePromocodeRequestDTO { PromoCodeId = promoId });
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task DeletePromoCode_EmptyGuid_Throws400()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeletePromoCode(new DeletePromocodeRequestDTO { PromoCodeId = Guid.Empty }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task DeletePromoCode_NotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeletePromoCode(new DeletePromocodeRequestDTO { PromoCodeId = Guid.NewGuid() }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeletePromoCode_SoftDeletes_NotReturnedInGetAll()
        {
            var context = GetDbContext();
            var service = GetService(context);

            await service.AddPromoCode(ValidRequest("SOFT"));
            var promoId = context.PromoCodes.First().PromoCodeId;
            await service.DeletePromoCode(new DeletePromocodeRequestDTO { PromoCodeId = promoId });

            var result = await service.GetAllPromocode(new GetAllPromocodeRequestDTO { Pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.Empty(result.Data.PromoCodes);
        }

        // ── GetAllPromocode ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllPromocode_WithData_ReturnsList()
        {
            var context = GetDbContext();
            var service = GetService(context);

            await service.AddPromoCode(ValidRequest("LIST10"));
            var result = await service.GetAllPromocode(new GetAllPromocodeRequestDTO { Pagination = new Pagination { PageNumber = 1, PageSize = 10 } });

            Assert.NotEmpty(result.Data.PromoCodes);
        }

        [Fact]
        public async Task GetAllPromocode_Empty_ReturnsEmptyList()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllPromocode(new GetAllPromocodeRequestDTO { Pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.Empty(result.Data.PromoCodes);
        }

        // ── VerifyPromoCode ──────────────────────────────────────────────────────

        [Fact]
        public async Task VerifyPromoCode_Valid_ReturnsIsValid()
        {
            var context = GetDbContext();
            var service = GetService(context);

            await service.AddPromoCode(ValidRequest("VERIFY10"));
            var result = await service.VerifyPromoCode(new VerifyPromoCodeRequestDTO { PromoCodeName = "VERIFY10" });

            Assert.True(result.Data.IsValid);
            Assert.Equal(10, result.Data.DiscountPercentage);
        }

        [Fact]
        public async Task VerifyPromoCode_Invalid_ReturnsIsValidFalse()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.VerifyPromoCode(new VerifyPromoCodeRequestDTO { PromoCodeName = "NOTEXIST" });
            Assert.False(result.Data.IsValid);
        }

        [Fact]
        public async Task VerifyPromoCode_Expired_ReturnsIsValidFalse()
        {
            var context = GetDbContext();
            context.PromoCodes.Add(new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromoCodeName = "EXPIRED",
                DiscountPercentage = 10,
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow.AddDays(-1)
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.VerifyPromoCode(new VerifyPromoCodeRequestDTO { PromoCodeName = "EXPIRED" });

            Assert.False(result.Data.IsValid);
        }

        [Fact]
        public async Task VerifyPromoCode_NotActiveYet_ReturnsIsValidFalse()
        {
            var context = GetDbContext();
            context.PromoCodes.Add(new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromoCodeName = "FUTURE",
                DiscountPercentage = 10,
                FromDate = DateTime.UtcNow.AddDays(5),
                ToDate = DateTime.UtcNow.AddDays(10)
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.VerifyPromoCode(new VerifyPromoCodeRequestDTO { PromoCodeName = "FUTURE" });

            Assert.False(result.Data.IsValid);
        }
    }
}
