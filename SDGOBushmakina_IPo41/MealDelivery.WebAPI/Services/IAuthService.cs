using MealDelivery.Models.Entities;
using System.Threading.Tasks;

namespace MealDelivery.WebAPI.Services
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(string login, string password);
        Task<User> CreateGuestUserAsync();
    }
}