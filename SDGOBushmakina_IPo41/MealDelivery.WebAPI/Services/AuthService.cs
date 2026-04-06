using MealDelivery.Data.Repositories;
using MealDelivery.Models.Entities;
using BC = BCrypt.Net.BCrypt;
using System;
using System.Threading.Tasks;

namespace MealDelivery.WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> AuthenticateAsync(string login, string password)
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null || !BC.Verify(password, user.PasswordHash) || !user.IsActive)
            {
                return null;
            }

            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return user;
        }

        public async Task<User> CreateGuestUserAsync()
        {
            var guestRole = await _userRepository.GetRoleByNameAsync("guest");
            if (guestRole == null)
            {
                throw new Exception("Роль гостя не найдена в системе");
            }

            var guestUser = new User
            {
                Login = $"guest_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                PasswordHash = BC.HashPassword("guest"),
                RoleId = guestRole.Id,
                FullName = "Гость",
                Email = $"guest_{DateTime.Now.Ticks}@example.com",
                Phone = "", // <--- ИСПРАВЛЕНИЕ: Передаем пустую строку
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(guestUser);
            return guestUser;
        }
    }
}