using MealDelivery.Models.Entities;
using MealDelivery.Data.Context;

namespace MealDelivery.Data.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(MealDeliveryDbContext context) : base(context) { }
    }
}