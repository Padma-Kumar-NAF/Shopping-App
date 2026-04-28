using ProductApi.Infrastructure.DependencyInjection;

namespace ProductApi.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            var app = builder.Build();
            app.UseInfrastructurePolicy();

            builder.Services.AddInfrastructure(builder.Configuration);

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
