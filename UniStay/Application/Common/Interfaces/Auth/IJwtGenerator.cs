using System;
using Domain.Users;

namespace Application.Common.Interfaces.Auth
{
    public interface IJwtGenerator
    {
        string GenerateToken(UserId userId, string email);
        // Можливо, інші перевантаження, наприклад, з ролями:
        // string GenerateToken(string userId, string email, IList<string> roles);
    }
}