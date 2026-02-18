using Microsoft.EntityFrameworkCore;
using ShoppingApp.Models;

namespace ShoppingApp.Contexts
{
    public class ShoppingContext : DbContext
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Stock> Stock { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }

        public ShoppingContext(DbContextOptions<ShoppingContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Primary key
            modelBuilder.Entity<Address>()
                .HasKey(a => a.AddressId)
                .HasName("PK_Address");

            modelBuilder.Entity<Cart>()
                .HasKey(c => c.CartId)
                .HasName("PK_Cart");

            modelBuilder.Entity<CartItem>()
                .HasKey(ci => ci.Id)
                .HasName("PK_CartItem");

            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryId)
                .HasName("PK_Category");

            modelBuilder.Entity<Log>()
                .HasKey(l => l.LogId)
                .HasName("PK_Log");


            modelBuilder.Entity<OrderDetails>()
                .HasKey(l => l.OrderDetailsId)
                .HasName("PK_OrderDetails");

            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId)
                .HasName("PK_Product");

            modelBuilder.Entity<Review>()
                .HasKey(p => p.ReviewId)
                .HasName("PK_Review");

            modelBuilder.Entity<Stock>()
                .HasKey(u => u.StockId)
                .HasName("PK_Stock");

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId)
                .HasName("PK_User");

            modelBuilder.Entity<UserDetails>()
                .HasKey(u => u.UserDetailsId)
                .HasName("PK_UserDetails");



            // Relations
            modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .HasConstraintName("FK_Address_User")
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .HasConstraintName("FK_Cart_User")
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .HasConstraintName("FK_CartItem_Cart")
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .HasConstraintName("FK_Product_Category")
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Log>()
            .HasOne(l => l.User)
            .WithMany(u => u.Logs)
            .HasForeignKey(l => l.UserId)
            .HasConstraintName("FK_Log_User")
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(l => l.OrderId)
                .HasName("PK_Order");

                entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .HasConstraintName("FK_Order_User")
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Address)
                .WithMany(a => a.Orders)
                .HasForeignKey(o => o.AddressId)
                .HasConstraintName("FK_Order_Address")
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .HasConstraintName("FK_OrderDetails_Order")
                .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
