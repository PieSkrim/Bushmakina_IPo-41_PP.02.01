using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MealDelivery.Models.Entities;

namespace MealDelivery.WebAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            // Используем значения по умолчанию, если файл appsettings.json не найден или пустой!
            var secretKey = _configuration["JwtSettings:Secret"] ?? "Super_Secret_Key_For_Meal_Delivery_App_Min_32_Chars!";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "MealDeliveryAPI";
            var audience = _configuration["JwtSettings:Audience"] ?? "MealDeliveryClient";
            var expMinutesStr = _configuration["JwtSettings:ExpirationMinutes"] ?? "60";

            if (!double.TryParse(expMinutesStr, out double expMinutes)) expMinutes = 60;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Защита от пустых значений, чтобы программа не падала
            var roleName = user.Role?.Name ?? "guest";
            var fullName = user.FullName ?? "Пользователь";
            var email = user.Email ?? string.Empty;
            var phone = user.Phone ?? string.Empty;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Login ?? "unknown"),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("FullName", fullName),
                    new Claim("Email", email),
                    new Claim("Phone", phone)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}