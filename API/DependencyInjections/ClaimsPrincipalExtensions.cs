using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Enums;

namespace API.DependencyInjections;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(id))
        {
            throw new UnauthorizedAccessException("User id claim is missing.");
        }

        return Guid.Parse(id);
    }

    public static UserRole GetUserRole(this ClaimsPrincipal user)
    {
        var role = user.FindFirstValue(ClaimTypes.Role)
            ?? user.FindFirstValue("role");

        if (string.IsNullOrEmpty(role))
        {
            throw new UnauthorizedAccessException("User role claim is missing.");
        }

        return Enum.Parse<UserRole>(role);
    }

    public static string GetUserEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? user.FindFirstValue(ClaimTypes.Email)
        ?? throw new UnauthorizedAccessException("User email claim is missing.");
}
