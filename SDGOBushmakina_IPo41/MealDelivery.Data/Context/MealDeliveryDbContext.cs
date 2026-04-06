using Microsoft.EntityFrameworkCore;
using MealDelivery.Models.Entities;
using MealDelivery.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection.Emit;

namespace MealDelivery.Data.Context
{
    public class MealDeliveryDbContext : DbContext
    {
        public MealDeliveryDbContext(DbContextOptions<MealDeliveryDbContext> options) : base(options)
        {
        }

        public DbSet<Meal> Meals { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("mealdelivery");

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.GetTableName().ToLower());
            }

            modelBuilder.Entity<Meal>(entity =>
            {
                entity.Property(m => m.Id).HasColumnName("id");
                entity.Property(m => m.Name).HasColumnName("name");
                entity.Property(m => m.CategoryId).HasColumnName("category_id");
                entity.Property(m => m.Description).HasColumnName("description");
                entity.Property(m => m.ManufacturerId).HasColumnName("manufacturer_id");
                entity.Property(m => m.SupplierId).HasColumnName("supplier_id");
                entity.Property(m => m.Price).HasColumnName("price");
                entity.Property(m => m.Unit).HasColumnName("unit");
                entity.Property(m => m.StockQuantity).HasColumnName("stock_quantity");
                entity.Property(m => m.Discount).HasColumnName("discount");
                entity.Property(m => m.ImagePath).HasColumnName("image_path");
                entity.Property(m => m.IsActive).HasColumnName("is_active");
                entity.Property(m => m.CreatedAt).HasColumnName("created_at");
                entity.Property(m => m.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.Name).HasColumnName("name");
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.CreatedAt).HasColumnName("created_at");
                entity.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.Property(m => m.Id).HasColumnName("id");
                entity.Property(m => m.Name).HasColumnName("name");
                entity.Property(m => m.Address).HasColumnName("address");
                entity.Property(m => m.Phone).HasColumnName("phone");
                entity.Property(m => m.Email).HasColumnName("email");
                entity.Property(m => m.CreatedAt).HasColumnName("created_at");
                entity.Property(m => m.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(s => s.Id).HasColumnName("id");
                entity.Property(s => s.Name).HasColumnName("name");
                entity.Property(s => s.ContactPerson).HasColumnName("contact_person");
                entity.Property(s => s.Phone).HasColumnName("phone");
                entity.Property(s => s.Email).HasColumnName("email");
                entity.Property(s => s.Address).HasColumnName("address");
                entity.Property(s => s.CreatedAt).HasColumnName("created_at");
                entity.Property(s => s.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.Id).HasColumnName("id");
                entity.Property(o => o.UserId).HasColumnName("user_id");
                entity.Property(o => o.OrderDate).HasColumnName("order_date");
                entity.Property(o => o.TotalAmount).HasColumnName("total_amount");
                entity.Property(o => o.Status).HasColumnName("status");
                entity.Property(o => o.DeliveryAddress).HasColumnName("delivery_address");
                entity.Property(o => o.DeliveryTime).HasColumnName("delivery_time");
                entity.Property(o => o.PaymentMethod).HasColumnName("payment_method");
                entity.Property(o => o.Notes).HasColumnName("notes");
                entity.Property(o => o.CreatedAt).HasColumnName("created_at");
                entity.Property(o => o.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.Property(od => od.Id).HasColumnName("id");
                entity.Property(od => od.OrderId).HasColumnName("order_id");
                entity.Property(od => od.MealId).HasColumnName("meal_id");
                entity.Property(od => od.Quantity).HasColumnName("quantity");
                entity.Property(od => od.PriceAtPurchase).HasColumnName("price_at_purchase");
                entity.Property(od => od.DiscountAtPurchase).HasColumnName("discount_at_purchase");
                entity.Property(od => od.CreatedAt).HasColumnName("created_at");
                entity.Property(od => od.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasColumnName("id");
                entity.Property(u => u.Login).HasColumnName("login");
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
                entity.Property(u => u.RoleId).HasColumnName("role_id");
                entity.Property(u => u.FullName).HasColumnName("full_name");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.Phone).HasColumnName("phone");
                entity.Property(u => u.IsActive).HasColumnName("is_active");
                entity.Property(u => u.LastLogin).HasColumnName("last_login");
                entity.Property(u => u.CreatedAt).HasColumnName("created_at");
                entity.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Id).HasColumnName("id");
                entity.Property(r => r.Name).HasColumnName("name");
                entity.Property(r => r.Description).HasColumnName("description");
                entity.Property(r => r.CreatedAt).HasColumnName("created_at");
                entity.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            });

            // Отношения
            modelBuilder.Entity<Meal>()
                .HasOne(m => m.Category)
                .WithMany()
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meal>()
                .HasOne(m => m.Manufacturer)
                .WithMany()
                .HasForeignKey(m => m.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meal>()
                .HasOne(m => m.Supplier)
                .WithMany()
                .HasForeignKey(m => m.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany()
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Meal)
                .WithMany()
                .HasForeignKey(od => od.MealId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ENUM типы
            modelBuilder.HasPostgresEnum<OrderStatus>("order_status");
            modelBuilder.HasPostgresEnum<UserRole>("user_role");

            // Индексы
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<Meal>()
                .HasIndex(m => m.Name);

            modelBuilder.Entity<Meal>()
                .HasIndex(m => m.CategoryId);

            modelBuilder.Entity<Meal>()
                .HasIndex(m => m.ManufacturerId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<OrderDetail>()
                .HasIndex(od => od.MealId);

            // Начальные данные для ролей
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "guest", Description = "Гость (неавторизованный пользователь)" },
                new Role { Id = 2, Name = "client", Description = "Авторизованный клиент" },
                new Role { Id = 3, Name = "manager", Description = "Менеджер" },
                new Role { Id = 4, Name = "admin", Description = "Администратор" }
            );

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=bushmakina;Username=app;Password=123456789;SearchPath=mealdelivery");
            }
        }
    }
}