using MealDelivery.Models.Entities;
using MealDelivery.Data.Context;

namespace MealDelivery.Data.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(MealDeliveryDbContext context) : base(context) { }
    }
}