using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MealDelivery.WebAPI.Extensions;
using MealDelivery.WebAPI.Middleware;
using MealDelivery.Data;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.Json;
using MealDelivery.Data.Context;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using System.IO;
using MealDelivery.Models.Entities;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic);
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;

builder.Services.ConfigureDatabase(configuration)
    .ConfigureJwtAuthentication(configuration)
    .ConfigureAuthorization()
    .ConfigureCors()
    .ConfigureSwagger()
    .RegisterRepositories()
    .RegisterServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (Directory.Exists(webRootPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(webRootPath),
        RequestPath = ""
    });
}
else
{
    app.UseStaticFiles();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandling();
app.MapControllers();
app.MapFallbackToFile("index.html");

await SeedDatabaseAsync(app);

app.Run();

async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MealDeliveryDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        dbContext.Database.EnsureCreated();

        // 1. Роли
        if (!dbContext.Roles.Any())
        {
            dbContext.Roles.AddRange(new[]
            {
                new Role { Id = 1, Name = "guest", Description = "Гость (неавторизованный пользователь)" },
                new Role { Id = 2, Name = "client", Description = "Авторизованный клиент" },
                new Role { Id = 3, Name = "manager", Description = "Менеджер" },
                new Role { Id = 4, Name = "admin", Description = "Администратор" }
            });
            await dbContext.SaveChangesAsync();
        }

        // 2. Администратор
        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "admin");
        if (adminRole != null && !dbContext.Users.Any(u => u.RoleId == adminRole.Id))
        {
            var adminUser = new User
            {
                Login = "admin",
                PasswordHash = BC.HashPassword("admin"),
                RoleId = adminRole.Id,
                FullName = "Администратор системы",
                Email = "admin@example.com",
                Phone = "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        // 3. Базовые Категории
        if (!dbContext.Categories.Any())
        {
            dbContext.Categories.AddRange(
                new Category { Name = "Супы", Description = "Горячие первые блюда", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Горячее", Description = "Вторые блюда из мяса и птицы", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Салаты", Description = "Свежие овощи и закуски", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await dbContext.SaveChangesAsync();
        }

        // 4. Базовые Производители
        if (!dbContext.Manufacturers.Any())
        {
            dbContext.Manufacturers.AddRange(
                new Manufacturer { Name = "Кухня на районе", Address = "г. Москва, ул. Ленина, 1", Phone = "88001002030", Email = "info@kuhnya.ru", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Manufacturer { Name = "Милти", Address = "г. Москва, пр. Мира, 15", Phone = "88002003040", Email = "hello@mealty.ru", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Manufacturer { Name = "Grow Food", Address = "г. Москва, ул. Тверская, 10", Phone = "88003004050", Email = "support@growfood.ru", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await dbContext.SaveChangesAsync();
        }

        // 5. Базовые Поставщики
        if (!dbContext.Suppliers.Any())
        {
            dbContext.Suppliers.AddRange(
                new Supplier { Name = "ООО Овощной Базар", ContactPerson = "Иван Иванов", Phone = "89001112233", Email = "ivan@ovoshi.ru", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Supplier { Name = "Мясной Двор", ContactPerson = "Петр Петров", Phone = "89002223344", Email = "petr@meat.ru", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Supplier { Name = "Эко-Ферма", ContactPerson = "Анна Смирнова", Phone = "89003334455", Email = "anna@eco.ru", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation("База данных успешно инициализирована (с базовыми данными)");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при инициализации базы данных");
    }
}