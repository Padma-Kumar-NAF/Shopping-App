using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Middleware;
using ShoppingApp.Models;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using System.Text;

namespace ShoppingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200",
                              "http://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            builder.Services.AddDbContext<ShoppingContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Development"));
            });

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "ShoppingApp",
                        ValidAudience = "ShoppingAppUsers",
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("ThisIsMySuperSecureKey123456789PadmaKumar"))
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<ICartItemsService, CartItemsService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IUserDetailsService, UserDetailsService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IWishListService, WishListService > ();

            var app = builder.Build();
            app.UseCors("AllowAngular");

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseExceptionHandler("/error");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseIpRateLimiting();

            app.UseMiddleware<ExceptionMiddleware>();

            app.MapControllers();
            app.Run();
        }
    }
}
