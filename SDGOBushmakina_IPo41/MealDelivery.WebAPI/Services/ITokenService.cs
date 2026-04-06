using MealDelivery.Models.Entities;

namespace MealDelivery.WebAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}