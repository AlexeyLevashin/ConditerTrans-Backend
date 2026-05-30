using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Enums;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this List<Claim> claims)
    {
        var id = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        return string.IsNullOrEmpty(id) ? throw new UnauthorizedAccessException("User id claim is missing.") : Guid.Parse(id);
    }

    public static UserRole GetUserRole(this List<Claim> claims)
    {
        var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        return string.IsNullOrEmpty(role) ? throw new UnauthorizedAccessException("User role claim is missing.") : Enum.Parse<UserRole>(role);
    }

    public static string GetUserEmail(this List<Claim> claims) =>
        claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? throw new UnauthorizedAccessException("User email claim is missing.");
}
