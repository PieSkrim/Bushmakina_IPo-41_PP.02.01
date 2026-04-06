using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MealDelivery.Data.Context;
using System;
using System.Threading.Tasks;

namespace MealDelivery.Data
{
    public static class DatabaseHelper
    {
        public static bool TestConnection(IConfiguration configuration)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<MealDeliveryDbContext>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseNpgsql(connectionString);

                using var context = new MealDeliveryDbContext(optionsBuilder.Options);
                context.Database.OpenConnection();
                context.Database.CloseConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task InitializeDatabaseAsync(IConfiguration configuration)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<MealDeliveryDbContext>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseNpgsql(connectionString, options =>
                {
                    options.MigrationsHistoryTable("__EFMigrationsHistory", "mealdelivery");
                });

                using var context = new MealDeliveryDbContext(optionsBuilder.Options);
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при инициализации базы данных: {ex.Message}");
            }
        }

        public static string GetConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString("DefaultConnection");
        }
    }
}