using System.Collections.Generic;
using System.Threading.Tasks;
using MealDelivery.Models.Entities;
using System;

namespace MealDelivery.Data.Repositories
{
    public interface IMealRepository : IGenericRepository<Meal>
    {
        Task<IEnumerable<Meal>> GetFilteredMealsAsync(
            string searchTerm = null,
            int? manufacturerId = null,
            string sortBy = null,
            bool ascending = true);

        Task<bool> CategoryExistsAsync(int categoryId);
        Task<bool> ManufacturerExistsAsync(int manufacturerId);
        Task<bool> SupplierExistsAsync(int supplierId);

        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Manufacturer>> GetManufacturersAsync();
        Task<IEnumerable<Supplier>> GetSuppliersAsync();

        Task<bool> CanDeleteMealAsync(int mealId);

        Task<IEnumerable<Meal>> GetMealsByIdsAsync(List<int> mealIds);
    }
}