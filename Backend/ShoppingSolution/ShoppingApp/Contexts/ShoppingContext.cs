using Microsoft.EntityFrameworkCore;
using ShoppingApp.Models;

namespace ShoppingApp.Contexts
{
    public class ShoppingContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public ShoppingContext(DbContextOptions<ShoppingContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.UserId) /// Add a datetime default
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        }
    }
}
