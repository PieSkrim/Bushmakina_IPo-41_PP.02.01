using Microsoft.EntityFrameworkCore;
using MealDelivery.Models.Entities;
using MealDelivery.Data.Context;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MealDelivery.Data.Repositories
{
    public class MealRepository : IMealRepository
    {
        private readonly MealDeliveryDbContext _context;

        public MealRepository(MealDeliveryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Meal>> GetAllAsync()
        {
            return await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Manufacturer)
                .Include(m => m.Supplier)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Meal> GetByIdAsync(int id)
        {
            return await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Manufacturer)
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddAsync(Meal entity)
        {
            await _context.Meals.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Meal entity)
        {
            _context.Meals.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var meal = await GetByIdAsync(id);
            if (meal != null)
            {
                _context.Meals.Remove(meal);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Meals.AnyAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Meal>> FindAsync(Expression<Func<Meal, bool>> predicate)
        {
            return await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Manufacturer)
                .Include(m => m.Supplier)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Meal>> GetFilteredMealsAsync(
            string searchTerm = null,
            int? manufacturerId = null,
            string sortBy = null,
            bool ascending = true)
        {
            var query = _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Manufacturer)
                .Include(m => m.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(searchLower) ||
                    m.Category.Name.ToLower().Contains(searchLower) ||
                    m.Description.ToLower().Contains(searchLower) ||
                    m.Manufacturer.Name.ToLower().Contains(searchLower) ||
                    (m.Supplier != null && m.Supplier.Name.ToLower().Contains(searchLower)));
            }

            if (manufacturerId.HasValue && manufacturerId.Value > 0)
            {
                query = query.Where(m => m.ManufacturerId == manufacturerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "stock":
                        query = ascending ? query.OrderBy(m => m.StockQuantity) : query.OrderByDescending(m => m.StockQuantity);
                        break;
                    case "price":
                        query = ascending ? query.OrderBy(m => m.Price) : query.OrderByDescending(m => m.Price);
                        break;
                    case "name":
                        query = ascending ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name);
                        break;
                    case "discount":
                        query = ascending ? query.OrderBy(m => m.Discount) : query.OrderByDescending(m => m.Discount);
                        break;
                    default:
                        query = query.OrderBy(m => m.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(m => m.Name);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.Id == categoryId);
        }

        public async Task<bool> ManufacturerExistsAsync(int manufacturerId)
        {
            return await _context.Manufacturers.AnyAsync(m => m.Id == manufacturerId);
        }

        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == supplierId);
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Manufacturer>> GetManufacturersAsync()
        {
            return await _context.Manufacturers
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersAsync()
        {
            return await _context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> CanDeleteMealAsync(int mealId)
        {
            return !await _context.OrderDetails.AnyAsync(od => od.MealId == mealId);
        }

        public async Task<IEnumerable<Meal>> GetMealsByIdsAsync(List<int> mealIds)
        {
            return await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Manufacturer)
                .Include(m => m.Supplier)
                .Where(m => mealIds.Contains(m.Id))
                .ToListAsync();
        }
    }
}