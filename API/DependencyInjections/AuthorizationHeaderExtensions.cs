using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace API.DependencyInjections;

public static class AuthorizationHeaderExtensions
{
    private const string PrincipalItemKey = "ValidatedBearerPrincipal";

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

    public static ClaimsPrincipal GetPrincipalFromAuthorizationHeader(
        this HttpRequest request,
        TokenValidationParameters validationParameters)
    {
        if (request.HttpContext.Items.TryGetValue(PrincipalItemKey, out var cached)
            && cached is ClaimsPrincipal principal)
        {
            return principal;
        }

        var token = request.GetBearerToken();
        var handler = new JwtSecurityTokenHandler();

        principal = handler.ValidateToken(token, validationParameters, out _);
        request.HttpContext.Items[PrincipalItemKey] = principal;

        return principal;
    }
}
