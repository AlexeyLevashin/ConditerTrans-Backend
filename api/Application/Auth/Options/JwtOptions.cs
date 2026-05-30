namespace Application.Auth.Options;

public class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    public int AccessTokenExpiresDays { get; set; } = 7;
    public int RefreshTokenExpiresDays { get; set; } = 30;
}