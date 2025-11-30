using System;
using Domain.Users;

namespace Application.Common.Interfaces.Auth
{
    public interface IJwtGenerator
    {
        string GenerateToken(UserId userId, string email, string role);
    }
}