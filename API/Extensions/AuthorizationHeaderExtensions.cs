using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Extensions;

public static class AuthorizationHeaderExtensions
{
    public static string GetBearerToken(this HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var headerValue))
        {
            throw new UnauthorizedAccessException("Authorization header is missing.");
        }

        var header = headerValue.ToString().Trim();

        const string bearerPrefix = "Bearer ";
        if (!header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Authorization header must be: Bearer <token>.");
        }

        var token = header[bearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Bearer token is empty.");
        }

        return token;
    }

    public static IEnumerable<Claim> GetPrincipalFromAuthorizationHeader(this HttpRequest request)
    {
        
        var token = request.GetBearerToken();
        var handler = new JwtSecurityTokenHandler();
        var t = handler.ReadJwtToken(token);
        
        return t is null ? throw new UnauthorizedAccessException("Bearer token is empty.") : t.Claims;
    }
}