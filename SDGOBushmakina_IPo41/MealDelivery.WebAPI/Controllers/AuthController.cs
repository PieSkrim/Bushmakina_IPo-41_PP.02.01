using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using MealDelivery.Data.Repositories;
using MealDelivery.Models.DTOs.Requests;
using MealDelivery.Models.DTOs.Responses;
using MealDelivery.Models.Entities;
using MealDelivery.WebAPI.Services;
using BC = BCrypt.Net.BCrypt;

namespace MealDelivery.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthController(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(new { message = "Некорректный запрос" });

                var login = request.Login?.Trim();
                var password = request.Password?.Trim();

                var user = await _userRepository.GetByLoginAsync(login);

                // АВАРИЙНЫЙ ЛЮК ДЛЯ АДМИНА
                if (login == "admin" && password == "admin")
                {
                    if (user == null)
                    {
                        var adminRole = await _userRepository.GetRoleByNameAsync("admin");
                        if (adminRole == null)
                            return BadRequest(new { message = "ОШИБКА: Роль 'admin' не создана в БД!" });

                        user = new User
                        {
                            Login = "admin",
                            PasswordHash = BC.HashPassword("admin"),
                            RoleId = adminRole.Id,
                            FullName = "Администратор системы",
                            Email = "admin@example.com",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            LastLogin = DateTime.UtcNow
                        };

                        await _userRepository.AddAsync(user);
                        user = await _userRepository.GetByLoginAsync(login); // Перезагружаем с подтянутой ролью
                    }
                    else
                    {
                        // Админ есть в БД -> просто чиним пароль и обновляем дату входа за ОДИН запрос
                        user.PasswordHash = BC.HashPassword("admin");
                        user.IsActive = true;
                        user.LastLogin = DateTime.UtcNow;
                        await _userRepository.UpdateAsync(user);
                    }
                }
                else
                {
                    // Проверка для обычных пользователей
                    if (user == null || !BC.Verify(password, user.PasswordHash) || !user.IsActive)
                    {
                        return Unauthorized(new { message = "Неверные учетные данные или учетная запись неактивна" });
                    }

                    user.LastLogin = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                }

                // Генерация токена
                var token = _tokenService.GenerateToken(user);

                return Ok(new AuthResponse
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Login = user.Login,
                        FullName = user.FullName,
                        RoleName = user.Role?.Name,
                        Email = user.Email,
                        Phone = user.Phone
                    }
                });
            }
            catch (Exception ex)
            {
                // ВЫТАСКИВАЕМ ВНУТРЕННЮЮ ОШИБКУ БАЗЫ ДАННЫХ (Inner Exception)
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | ДЕТАЛИ: " + ex.InnerException.Message;
                }
                return StatusCode(500, new { message = $"ОШИБКА БД: {errorMsg}" });
            }
        }

        [HttpPost("guest")]
        [AllowAnonymous]
        public async Task<IActionResult> GuestLogin()
        {
            try
            {
                var guestRole = await _userRepository.GetRoleByNameAsync("guest");
                if (guestRole == null)
                    return BadRequest(new { message = "ОШИБКА: Роль 'guest' не создана в БД!" });

                var guestLogin = $"guest_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                var guestUser = new User
                {
                    Login = guestLogin,
                    PasswordHash = BC.HashPassword("guest"),
                    RoleId = guestRole.Id,
                    FullName = "Гость",
                    Email = $"{guestLogin}@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(guestUser);
                guestUser = await _userRepository.GetByLoginAsync(guestLogin);

                var token = _tokenService.GenerateToken(guestUser);

                return Ok(new AuthResponse
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = guestUser.Id,
                        Login = guestUser.Login,
                        FullName = guestUser.FullName,
                        RoleName = guestUser.Role?.Name
                    }
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null) errorMsg += " | ДЕТАЛИ: " + ex.InnerException.Message;
                return StatusCode(500, new { message = $"ОШИБКА БД: {errorMsg}" });
            }
        }

        [HttpGet("current")]
        [Authorize] // ИСПРАВЛЕНИЕ: Добавлен атрибут авторизации
        public IActionResult GetCurrentUserInfo()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var user = _userRepository.GetByIdAsync(userId).Result;
            if (user == null) return NotFound();

            return Ok(new CurrentUserResponse
            {
                Id = user.Id,
                Login = user.Login,
                FullName = user.FullName,
                Role = user.Role?.Name,
                Email = user.Email,
                Phone = user.Phone
            });
        }
    }
}