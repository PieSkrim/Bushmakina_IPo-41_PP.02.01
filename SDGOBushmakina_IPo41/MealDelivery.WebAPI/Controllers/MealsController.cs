using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MealDelivery.Data.Repositories;
using MealDelivery.Models.Entities;
using MealDelivery.Models.DTOs.Requests;
using MealDelivery.Models.DTOs.Responses;
using MealDelivery.Models.Enums;
using ImageSharp = SixLabors.ImageSharp.Image;

namespace MealDelivery.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MealsController : ControllerBase
    {
        private readonly IMealRepository _mealRepository;
        private readonly IManufacturerRepository _manufacturerRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MealsController(
            IMealRepository mealRepository,
            IManufacturerRepository manufacturerRepository,
            ICategoryRepository categoryRepository,
            ISupplierRepository supplierRepository,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _mealRepository = mealRepository;
            _manufacturerRepository = manufacturerRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetMeals(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int manufacturerId = 0,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == null)
                {
                    userRole = "guest";
                }

                var isManagerOrAdmin = userRole == "manager" || userRole == "admin";

                var meals = await _mealRepository.GetFilteredMealsAsync(
                    searchTerm,
                    manufacturerId > 0 ? manufacturerId : null,
                    sortBy,
                    ascending);

                var manufacturers = isManagerOrAdmin ?
                    await _manufacturerRepository.GetAllAsync() :
                    Enumerable.Empty<Manufacturer>();

                var manufacturersList = manufacturers.Select(m => new FilterOption { Id = m.Id, Name = m.Name }).ToList();
                manufacturersList.Insert(0, new FilterOption { Id = 0, Name = "Все производители" });

                var result = meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Name = m.Name,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category.Name,
                    ManufacturerId = m.ManufacturerId,
                    ManufacturerName = m.Manufacturer.Name,
                    SupplierId = m.SupplierId,
                    SupplierName = m.Supplier?.Name,
                    Description = m.Description,
                    Price = m.Price,
                    Unit = m.Unit,
                    StockQuantity = m.StockQuantity,
                    Discount = m.Discount,
                    ImagePath = m.ImagePath,
                    ImageUrl = !string.IsNullOrEmpty(m.ImagePath) ?
                        $"{Request.Scheme}://{Request.Host}/images/{Path.GetFileName(m.ImagePath)}" :
                        $"{Request.Scheme}://{Request.Host}/images/picture.png",
                    IsActive = m.IsActive,
                    FinalPrice = m.CalculateFinalPrice(),
                    FormattedPrice = $"{m.Price:N2} руб.",
                    FormattedDiscount = m.Discount > 0 ? $"{m.Discount}% скидка" : string.Empty,
                    StockStatus = m.StockQuantity > 0 ? $"{m.StockQuantity} {m.Unit} в наличии" : "Нет на складе",
                    HighlightColor = GetHighlightColor(m.Discount, m.StockQuantity)
                }).ToList();

                return Ok(new MealsListResponse
                {
                    Meals = result,
                    Manufacturers = manufacturersList,
                    UserRole = userRole,
                    CanEdit = userRole == "admin",
                    CanFilter = isManagerOrAdmin
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при получении списка блюд: {errorMsg}" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMeal(int id)
        {
            try
            {
                var meal = await _mealRepository.GetByIdAsync(id);

                if (meal == null)
                {
                    return NotFound(new { message = "Блюдо не найдено" });
                }

                return Ok(new MealResponse
                {
                    Id = meal.Id,
                    Name = meal.Name,
                    CategoryId = meal.CategoryId,
                    CategoryName = meal.Category.Name,
                    ManufacturerId = meal.ManufacturerId,
                    ManufacturerName = meal.Manufacturer.Name,
                    SupplierId = meal.SupplierId,
                    SupplierName = meal.Supplier?.Name,
                    Description = meal.Description,
                    Price = meal.Price,
                    Unit = meal.Unit,
                    StockQuantity = meal.StockQuantity,
                    Discount = meal.Discount,
                    ImagePath = meal.ImagePath,
                    ImageUrl = !string.IsNullOrEmpty(meal.ImagePath) ?
                        $"{Request.Scheme}://{Request.Host}/images/{Path.GetFileName(meal.ImagePath)}" :
                        $"{Request.Scheme}://{Request.Host}/images/picture.png",
                    FinalPrice = meal.CalculateFinalPrice()
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при получении блюда: {errorMsg}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateMeal([FromBody] MealCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (!await _mealRepository.CategoryExistsAsync(request.CategoryId))
                    return BadRequest("Категория не найдена");

                if (!await _mealRepository.ManufacturerExistsAsync(request.ManufacturerId))
                    return BadRequest("Производитель не найден");

                if (request.SupplierId.HasValue && !await _mealRepository.SupplierExistsAsync(request.SupplierId.Value))
                    return BadRequest("Поставщик не найден");

                // ИСПРАВЛЕНИЕ: Задаем пустую строку по умолчанию, чтобы избежать ошибки NOT NULL
                string imagePath = string.Empty;

                if (!string.IsNullOrEmpty(request.Base64Image))
                {
                    imagePath = await SaveMealImageAsync(request.Base64Image, null) ?? string.Empty;
                }

                var meal = new Meal
                {
                    Name = request.Name,
                    CategoryId = request.CategoryId,
                    Description = request.Description ?? string.Empty, // ИСПРАВЛЕНИЕ: Fallback на пустую строку
                    ManufacturerId = request.ManufacturerId,
                    SupplierId = request.SupplierId,
                    Price = request.Price,
                    Unit = request.Unit,
                    StockQuantity = request.StockQuantity,
                    Discount = request.Discount,
                    ImagePath = imagePath,
                    IsActive = request.IsActive
                };

                await _mealRepository.AddAsync(meal);

                return CreatedAtAction(nameof(GetMeal), new { id = meal.Id }, new
                {
                    meal.Id,
                    meal.Name,
                    message = "Блюдо успешно добавлено"
                });
            }
            catch (Exception ex)
            {
                // ИСПРАВЛЕНИЕ: Выводим InnerException для точного понимания ошибки БД
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | ДЕТАЛИ: " + ex.InnerException.Message;
                }
                return StatusCode(500, new { message = $"Ошибка при добавлении блюда: {errorMsg}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateMeal(int id, [FromBody] MealUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var meal = await _mealRepository.GetByIdAsync(id);

                if (meal == null)
                {
                    return NotFound(new { message = "Блюдо не найдено" });
                }

                if (!await _mealRepository.CategoryExistsAsync(request.CategoryId))
                    return BadRequest("Категория не найдена");

                if (!await _mealRepository.ManufacturerExistsAsync(request.ManufacturerId))
                    return BadRequest("Производитель не найден");

                if (request.SupplierId.HasValue && !await _mealRepository.SupplierExistsAsync(request.SupplierId.Value))
                    return BadRequest("Поставщик не найден");

                // ИСПРАВЛЕНИЕ: Защита от null
                string imagePath = meal.ImagePath ?? string.Empty;

                if (!string.IsNullOrEmpty(request.Base64Image) && request.Base64Image != meal.ImagePath)
                {
                    var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    if (!string.IsNullOrEmpty(meal.ImagePath) && !meal.ImagePath.EndsWith("picture.png"))
                    {
                        var oldImagePath = Path.Combine(webRootPath, "images", Path.GetFileName(meal.ImagePath));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    imagePath = await SaveMealImageAsync(request.Base64Image, meal.Id.ToString()) ?? string.Empty;
                }

                meal.Name = request.Name;
                meal.CategoryId = request.CategoryId;
                meal.Description = request.Description ?? string.Empty; // ИСПРАВЛЕНИЕ
                meal.ManufacturerId = request.ManufacturerId;
                meal.SupplierId = request.SupplierId;
                meal.Price = request.Price;
                meal.Unit = request.Unit;
                meal.StockQuantity = request.StockQuantity;
                meal.Discount = request.Discount;
                meal.ImagePath = imagePath;
                meal.IsActive = request.IsActive;

                await _mealRepository.UpdateAsync(meal);

                return Ok(new { message = "Блюдо успешно обновлено" });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при обновлении блюда: {errorMsg}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMeal(int id)
        {
            try
            {
                var meal = await _mealRepository.GetByIdAsync(id);
                if (meal == null)
                {
                    return NotFound(new { message = "Блюдо не найдено" });
                }

                if (!await _mealRepository.CanDeleteMealAsync(id))
                {
                    return BadRequest("Нельзя удалить блюдо, которое есть в заказах");
                }

                if (!string.IsNullOrEmpty(meal.ImagePath) && !meal.ImagePath.EndsWith("picture.png"))
                {
                    var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var imagePath = Path.Combine(webRootPath, "images", Path.GetFileName(meal.ImagePath));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                await _mealRepository.DeleteAsync(id);

                return Ok(new { message = "Блюдо успешно удалено" });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при удалении блюда: {errorMsg}" });
            }
        }

        // ---------------- СПРАВОЧНИКИ (GET) ----------------

        [HttpGet("categories")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _mealRepository.GetCategoriesAsync();
                return Ok(categories.Select(c => new { c.Id, c.Name }));
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при получении списка категорий: {errorMsg}" });
            }
        }

        [HttpGet("manufacturers")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetManufacturers()
        {
            try
            {
                var manufacturers = await _manufacturerRepository.GetAllAsync();
                return Ok(manufacturers.Select(m => new { m.Id, m.Name }));
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при получении списка производителей: {errorMsg}" });
            }
        }

        [HttpGet("suppliers")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                var suppliers = await _mealRepository.GetSuppliersAsync();
                return Ok(suppliers.Select(s => new { s.Id, s.Name }));
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при получении списка поставщиков: {errorMsg}" });
            }
        }

        // ---------------- СПРАВОЧНИКИ (POST - НОВЫЕ МЕТОДЫ) ----------------

        [HttpPost("categories")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            try
            {
                category.Description = category.Description ?? string.Empty;
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                await _categoryRepository.AddAsync(category);
                return Ok(new { message = "Категория успешно добавлена", id = category.Id });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при добавлении: {errorMsg}" });
            }
        }

        [HttpPost("manufacturers")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateManufacturer([FromBody] Manufacturer manufacturer)
        {
            try
            {
                manufacturer.Address = manufacturer.Address ?? string.Empty;
                manufacturer.Phone = manufacturer.Phone ?? string.Empty;
                manufacturer.Email = manufacturer.Email ?? string.Empty;
                manufacturer.CreatedAt = DateTime.UtcNow;
                manufacturer.UpdatedAt = DateTime.UtcNow;

                await _manufacturerRepository.AddAsync(manufacturer);
                return Ok(new { message = "Производитель успешно добавлен", id = manufacturer.Id });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при добавлении: {errorMsg}" });
            }
        }

        [HttpPost("suppliers")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateSupplier([FromBody] Supplier supplier)
        {
            try
            {
                supplier.ContactPerson = supplier.ContactPerson ?? string.Empty;
                supplier.Phone = supplier.Phone ?? string.Empty;
                supplier.Email = supplier.Email ?? string.Empty;
                supplier.Address = supplier.Address ?? string.Empty;
                supplier.CreatedAt = DateTime.UtcNow;
                supplier.UpdatedAt = DateTime.UtcNow;

                await _supplierRepository.AddAsync(supplier);
                return Ok(new { message = "Поставщик успешно добавлен", id = supplier.Id });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = $"Ошибка при добавлении: {errorMsg}" });
            }
        }

        // ---------------- ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ----------------

        private string GetHighlightColor(decimal discount, int stockQuantity)
        {
            if (stockQuantity == 0) return "#ADD8E6";
            if (discount > 15) return "#2E8B57";
            return "transparent";
        }

        private async Task<string> SaveMealImageAsync(string base64Image, string mealId = null)
        {
            if (string.IsNullOrEmpty(base64Image))
                return null;

            try
            {
                var base64Data = base64Image.Contains(',') ? base64Image.Split(',')[1] : base64Image;
                var imageBytes = Convert.FromBase64String(base64Data);

                using var image = ImageSharp.Load(imageBytes);
                var width = Math.Min(image.Width, 300);
                var height = Math.Min(image.Height, 200);

                image.Mutate(x => x.Resize(width, height));

                var fileName = mealId != null
                    ? $"meal_{mealId}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg"
                    : $"meal_{Guid.NewGuid().ToString("N").Substring(0, 8)}.jpg";

                // ИСПРАВЛЕНИЕ: Защита от NullReferenceException если wwwroot не существует на момент старта
                var webRootPath = _webHostEnvironment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var imagesPath = Path.Combine(webRootPath, "images");

                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var imagePath = Path.Combine(imagesPath, fileName);
                await image.SaveAsJpegAsync(imagePath);

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обработке изображения: {ex.Message}");
            }
        }
    }
}