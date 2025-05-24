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

        public JwtGenerator(IConfiguration config)
        {
            // Ключ для підпису токена
            // Переконайтеся, що "Jwt:Key" налаштований в appsettings.json і є достатньо довгим (мінімум 16 символів для HMACSHA256)
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        }

        public string GenerateToken(UserId userId, string email)
        {
            var userIdString = userId.ToString(); // перетворення UserId у string

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userIdString), // Subject
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userIdString)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = "yourdomain.com",
                Audience = "yourdomain.com"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}