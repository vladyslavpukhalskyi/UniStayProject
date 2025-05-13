// Файл: Infrastructure/Security/PasswordHasherService.cs
using Application.Common.Interfaces; // Ваш IPasswordHasher
using Microsoft.AspNetCore.Identity; // Потрібен NuGet пакет Microsoft.Extensions.Identity.Core
using System;

namespace Infrastructure.Security
{
    internal class AppUserPlaceholder { } // Допоміжний клас

    public class PasswordHasherService : IPasswordHasher
    {
        private readonly PasswordHasher<AppUserPlaceholder> _passwordHasher = new PasswordHasher<AppUserPlaceholder>();

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");
            }
            return _passwordHasher.HashPassword(new AppUserPlaceholder(), password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            {
                return false;
            }
            var result = _passwordHasher.VerifyHashedPassword(new AppUserPlaceholder(), hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}