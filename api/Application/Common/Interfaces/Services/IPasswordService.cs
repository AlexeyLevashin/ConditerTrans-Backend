namespace Application.Common.Interfaces.Services;

public interface IPasswordService
{
    public string Hash(string password);
    public bool Verify(string password, string hashedPassword);
    public string GeneratePassword();
}