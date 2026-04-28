using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApi.Infrastructure.Data
{
    public class ProductContext(DbContextOptions<ProductContext> options) : DbContext(options)
    {
        public DbSet<Product> Prodcuts { get; set; }
    }
}
