using System.Security.Claims;
using Domain.Entities;

namespace Application.Common.Interfaces.Services;

public interface IJwtProvider
{
    public string GenerateAccessToken(User user);
    public string GenerateRefreshToken(User user);
    bool TryGetClaims(string refreshToken, out ClaimsPrincipal? claims);
}