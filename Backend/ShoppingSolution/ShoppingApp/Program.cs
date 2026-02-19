
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.Repositories;
using ShoppingApp.Interfaces.Service;
using ShoppingApp.Interfaces.Services;
using ShoppingApp.Models;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using System.Numerics;

namespace ShoppingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();


            builder.Services.AddDbContext<ShoppingContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Development"));
            });

            #region Repositories
            builder.Services.AddScoped<IRepository<Guid, User>, Repository<Guid, User>>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            #endregion

            #region Services
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IUserService, UserService>();
            #endregion

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
