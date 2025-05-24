using Application.Common.Interfaces.Auth;
using BCrypt.Net; // Потрібно встановити NuGet пакет BCrypt.Net-Next

namespace Infrastructure.Services
{
    public class PasswordHasher : IPasswordHash
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // <<<< Використовуємо повну назву
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword); // <<<< Використовуємо повну назву
        }
    }
}