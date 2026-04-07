using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using ShoppingApp.Contexts;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.Services;
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
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "A sample ASP.NET Core Web API"
                });
            });

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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/all-logs.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 20)
                .WriteTo.File(
                    "logs/errors.txt",
                    rollingInterval: RollingInterval.Day,
                     outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
                )
                //.WriteTo.File(
                //    "Logs/log-.txt",
                //    rollingInterval: RollingInterval.Day,
                //    retainedFileCountLimit: 20
                //)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddAuthorization();

            builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IPromoCodeService, PromoCodeService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IWishListService, WishListService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IUserMonthlyProductLimit, UserMonthlyProductLimitService>();

            builder.Services.AddScoped<MyResultFilter>();
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<MyResultFilter>();
            });


            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.UseCors("AllowAngular");

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            //app.UseExceptionHandler("/error");

            app.UseHttpsRedirection();

            app.UseAuthentication();// Middleware
            app.UseAuthorization();// Filter

            app.UseIpRateLimiting();

            app.UseMiddleware<ExceptionMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}