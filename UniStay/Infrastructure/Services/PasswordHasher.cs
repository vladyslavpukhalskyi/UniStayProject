using Application.Common.Interfaces.Auth;
using BCrypt.Net;

namespace Infrastructure.Services
{
    public class PasswordHasher : IPasswordHash
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}