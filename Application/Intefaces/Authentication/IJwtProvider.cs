using System.Security.Claims;
using Domain.Entities;

namespace Application.Intefaces.Authentication;

public interface IJwtProvider
{
    public string GenerateAccessToken(User user);
    public string GenerateRefreshToken(User user);

    public ClaimsPrincipal? ValidateToken(string refreshToken);
}