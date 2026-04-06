using MealDelivery.Models.Entities;
using MealDelivery.Data.Context;
using System.Data;

namespace MealDelivery.Data.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(MealDeliveryDbContext context) : base(context) { }
    }
}