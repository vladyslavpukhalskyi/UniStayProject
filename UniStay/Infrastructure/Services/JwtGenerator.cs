using Application.Common.Interfaces.Auth;
using Microsoft.Extensions.Configuration; // Для IConfiguration
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Users;

namespace Infrastructure.Services
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;   // <<<< ДОДАНО
        private readonly string _audience; // <<<< ДОДАНО

        public JwtGenerator(IConfiguration config)
        {
            // Ключ для підпису токена
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not found in configuration")));
            
            // Отримуємо Issuer та Audience з конфігурації
            _issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not found in configuration");     // <<<< ДОДАНО
            _audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience not found in configuration"); // <<<< ДОДАНО
        }

        public string GenerateToken(UserId userId, string email)
        {
            var userIdString = userId.ToString(); // перетворення UserId у string

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userIdString), // Subject
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userIdString)
                // Можливо, також додайте роль, якщо ви її використовуєте для авторизації за ролями
                // new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _issuer,   // <<<< ЗМІНЕНО: Використовуємо значення з конфігурації
                Audience = _audience // <<<< ЗМІНЕНО: Використовуємо значення з конфігурації
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}