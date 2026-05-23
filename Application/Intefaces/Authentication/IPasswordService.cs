namespace Application.Intefaces.Authentication;

public interface IPasswordService
{
    public string Hash(string password);
    public bool Verify(string password, string hashedPassword);
    public string GeneratePassword();
}