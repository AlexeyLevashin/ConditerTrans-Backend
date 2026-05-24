namespace Contracts.Auth.Responses;

public class CreateCompanyAdminResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
