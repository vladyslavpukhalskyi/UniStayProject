namespace Application.Common.Interfaces.Auth
{
    public interface IPasswordHash
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }
}