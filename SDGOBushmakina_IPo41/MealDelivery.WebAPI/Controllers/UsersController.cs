using Microsoft.AspNetCore.Mvc;
using MealDelivery.Data.Repositories;
using MealDelivery.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BC = BCrypt.Net.BCrypt;
using MealDelivery.Models.DTOs.Requests;
using MealDelivery.Models.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MealDelivery.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            return Ok(new UsersListResponse
            {
                Users = users.Select(u => new UserResponse
                {
                    Id = u.Id,
                    Login = u.Login,
                    FullName = u.FullName,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                }).ToList()
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (currentUserRole != "admin" && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _userRepository.GetByIdWithRoleAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            return Ok(new UserResponse
            {
                Id = user.Id,
                Login = user.Login,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.Role.Name,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userRepository.ExistsByLoginAsync(request.Login))
            {
                return BadRequest("Пользователь с таким логином уже существует");
            }

            var role = await _userRepository.GetRoleByNameAsync(request.RoleName);
            if (role == null)
            {
                return BadRequest("Указана несуществующая роль");
            }

            var user = new User
            {
                Login = request.Login,
                PasswordHash = BC.HashPassword(request.Password),
                RoleId = role.Id,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserCreateResponse
            {
                Id = user.Id,
                Login = user.Login,
                FullName = user.FullName,
                RoleName = role.Name,
                Email = user.Email
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (currentUserRole != "admin" && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            if (currentUserRole == "admin" && !string.IsNullOrEmpty(request.RoleName))
            {
                var role = await _userRepository.GetRoleByNameAsync(request.RoleName);
                if (role == null)
                {
                    return BadRequest("Указана несуществующая роль");
                }
                user.RoleId = role.Id;
            }

            user.FullName = request.FullName ?? user.FullName;
            user.Email = request.Email ?? user.Email;
            user.Phone = request.Phone ?? user.Phone;
            user.UpdatedAt = DateTime.UtcNow;

            if (currentUserRole == "admin" && request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            await _userRepository.UpdateAsync(user);
            return Ok(new { message = "Данные пользователя успешно обновлены" });
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] PasswordUpdateRequest request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (currentUserId != id)
            {
                return Forbid();
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            if (!BC.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Неверный текущий пароль");
            }

            user.PasswordHash = BC.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "Пароль успешно обновлен" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (currentUserId == id)
            {
                return BadRequest("Нельзя удалить свою учетную запись");
            }

            if (user.Role.Name == "admin")
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (currentUserRole != "admin")
                {
                    return Forbid("Только администратор может удалять других администраторов");
                }
            }

            await _userRepository.DeleteAsync(id);
            return Ok(new { message = "Пользователь успешно удален" });
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActivateUser(int id, [FromBody] ActivationRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (currentUserId == id && !request.IsActive)
            {
                return BadRequest("Нельзя деактивировать свою учетную запись");
            }

            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return Ok(new
            {
                message = request.IsActive ?
                    "Пользователь успешно активирован" :
                    "Пользователь успешно деактивирован"
            });
        }
    }
}