using ECommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Application.Interfaces;
using OrderApi.Infrastructure.Data;
using OrderApi.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderApi.Infrastructure.DependencyInjection
{
    public static class ServceiContainer
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection servcies,IConfiguration config)
        {
            SharedServiceContainer.AddSharedServices<OrderDbContext>(servcies, config, config["MySerilog:FileName"]!);
            servcies.AddScoped<IOrder,OrderRepository>();
            return servcies;
        }

        public static IApplicationBuilder UseInfrastructurePolicy(this IApplicationBuilder app)
        {
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }
    }
}
