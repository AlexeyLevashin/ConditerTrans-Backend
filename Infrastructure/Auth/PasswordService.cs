using Application.Common.Interfaces.Services;

namespace Infrastructure.Auth;

public class PasswordService : IPasswordService
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public string GeneratePassword()
    {
        return Guid.NewGuid().ToString().Replace("-", "");
    }
}