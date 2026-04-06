using MealDelivery.Models.Entities;
using MealDelivery.Data.Context;

namespace MealDelivery.Data.Repositories
{
    public class ManufacturerRepository : GenericRepository<Manufacturer>, IManufacturerRepository
    {
        public ManufacturerRepository(MealDeliveryDbContext context) : base(context) { }
    }
}