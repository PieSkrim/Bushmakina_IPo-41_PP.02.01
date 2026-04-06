using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MealDelivery.Models.Entities;

namespace MealDelivery.Data.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByLoginAsync(string login);
        Task<bool> ExistsByLoginAsync(string login);
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<IEnumerable<User>> GetAllWithRolesAsync();
        Task<User> GetByIdWithRoleAsync(int id);
    }
}