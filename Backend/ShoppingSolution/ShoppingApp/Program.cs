using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoppingApp.Contexts;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Middleware;
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
            var _configuration = builder.Configuration;

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200", "http://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidateRequestAttribute>();
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
                    var jwtKey = _configuration["Jwt:Key"];
                    if (string.IsNullOrEmpty(jwtKey))
                    {
                        throw new Exception("JWT Key is missing in configuration!");
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _configuration["Jwt:Issuer"],
                        ValidAudience = _configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero 
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            builder.Services.AddScoped<IAddressService, AddressService>();
            //builder.Services.AddScoped<ICartItemsService, CartItemsService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserDetailsService, UserDetailsService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IWishListService, WishListService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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