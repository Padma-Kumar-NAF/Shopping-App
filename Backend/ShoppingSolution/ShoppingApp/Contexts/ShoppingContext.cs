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
        public DbSet<UserHash> UserHash { get; set; }

        // Store procedure
        public DbSet<Category> categoriesSP { get; set; }

        public ShoppingContext(DbContextOptions<ShoppingContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.AddressId)
                    .HasName("PK_Address");

                entity.Property(o => o.AddressId)
                    .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                     .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(a => a.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(a => a.UserId)
                    .HasConstraintName("FK_Address_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.Orders)
                    .WithOne(o => o.Address)
                    .HasForeignKey(o => o.AddressId)
                    .HasConstraintName("FK_Order_Address")
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.CartId)
                    .HasName("PK_Cart");

                entity.Property(o => o.CartId)
          .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(c => c.User)
                    .WithOne(u => u.Cart)
                    .HasForeignKey<Cart>(c => c.UserId)
                    .HasConstraintName("FK_Cart_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.CartItems)
                    .WithOne(ci => ci.Cart)
                    .HasForeignKey(ci => ci.CartId)
                    .HasConstraintName("FK_CartItem_Cart")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.CartItemId)
                    .HasName("PK_CartItem");

                entity.Property(o => o.CartItemId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(ci => ci.CartId)
                    .HasConstraintName("FK_CartItem_Cart")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ci => ci.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(ci => ci.ProductId)
                    .HasConstraintName("FK_CartItem_Product")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ci => new { ci.CartId, ci.ProductId })
                    .IsUnique()
                    .HasDatabaseName("UX_CartItem_Cart_Product");
            });



            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId)
                    .HasName("PK_Category");

                entity.Property(o => o.CategoryId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");


                entity.HasIndex(c => c.CategoryName)
                    .IsUnique()
                    .HasDatabaseName("UX_Category_Name");

                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .HasConstraintName("FK_Product_Category")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(l => l.LogId)
                    .HasName("PK_Log");

                entity.Property(o => o.LogId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(l => l.User)
                    .WithMany(u => u.Logs)
                    .HasForeignKey(l => l.UserId)
                    .HasConstraintName("FK_Log_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(l => l.UserId)
                    .HasDatabaseName("IX_Log_UserId");

                entity.HasIndex(l => l.CreatedAt)
                    .HasDatabaseName("IX_Log_CreatedAt");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(l => l.OrderId)
                .HasName("PK_Order");

                entity.Property(o => o.OrderId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

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
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderDetails>(entity =>
            {
                entity.HasKey(od => od.OrderDetailsId)
                    .HasName("PK_OrderDetails");

                entity.Property(o => o.OrderDetailsId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(od => od.ProductPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderId)
                    .HasConstraintName("FK_OrderDetails_Order")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(od => od.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(od => od.ProductId)
                    .HasConstraintName("FK_OrderDetails_Product")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(od => new { od.OrderId, od.ProductId })
                    .IsUnique()
                    .HasDatabaseName("UX_OrderDetails_Order_Product");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId)
                    .HasName("PK_Product");

                entity.Property(o => o.ProductId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .HasConstraintName("FK_Product_Category")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Stock)
                    .WithOne(s => s.Product)
                    .HasForeignKey<Product>(p => p.StockId)
                    .HasConstraintName("FK_Product_Stock")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.Name)
                    .HasDatabaseName("IX_Product_Name");

                entity.HasIndex(p => p.CategoryId)
                    .HasDatabaseName("IX_Product_CategoryId");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.ReviewId)
                    .HasName("PK_Review");

                entity.Property(o => o.ReviewId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .HasConstraintName("FK_Review_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .HasConstraintName("FK_Review_Product")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => new { r.UserId, r.ProductId })
                    .IsUnique()
                    .HasDatabaseName("UX_Review_User_Product");
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasKey(s => s.StockId)
                    .HasName("PK_Stock");

                entity.Property(o => o.StockId)
         .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(s => s.Product)
                    .WithOne(p => p.Stock)
                    .HasForeignKey<Stock>(s => s.ProductId)
                    .HasConstraintName("FK_Stock_Product")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => s.ProductId)
                    .IsUnique()
                    .HasDatabaseName("UX_Stock_Product");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId)
                .HasName("PK_UserId");

                entity.Property(o => o.UserId)
                      .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasMany(u => u.Addresses)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId)
                    .HasConstraintName("FK_Address_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .HasConstraintName("FK_Order_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Logs)
                    .WithOne(l => l.User)
                    .HasForeignKey(l => l.UserId)
                    .HasConstraintName("FK_Log_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Reviews)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.UserId)
                    .HasConstraintName("FK_Review_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Cart)
                    .WithOne(c => c.User)
                    .HasForeignKey<Cart>(c => c.UserId)
                    .HasConstraintName("FK_Cart_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.UserDetails)
                    .WithOne(ud => ud.User)
                    .HasForeignKey<UserDetails>(ud => ud.UserId)
                    .HasConstraintName("FK_UserDetails_User")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u=> u.UserHash)
                    .WithOne(c => c.User)
                    .HasForeignKey<UserHash>(c => c.UserId)
                    .HasConstraintName("FK_UserHash_User")
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<UserDetails>(entity =>
            {
                entity.HasKey(ud => ud.UserDetailsId)
                .HasName("PK_UserDetails");

                entity.Property(o => o.UserDetailsId)
                .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(u=> u.User)
                .WithOne(e=>e.UserDetails)
                .HasForeignKey<UserDetails>(e=>e.UserId)
                .HasConstraintName("FK_User_UserDetails")
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserHash>(entity =>
            {
                entity.HasKey(ud => ud.UserHashId)
                .HasName("PK_UserDetails");

                entity.Property(o => o.UserHashId)
                .HasDefaultValueSql("NEWID()");

                entity.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(u=> u.User)
                .WithOne(e=>e.UserHash)
                .HasForeignKey<UserHash>(e=>e.UserId)
                .HasConstraintName("FK_User_UserToken")
                .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}