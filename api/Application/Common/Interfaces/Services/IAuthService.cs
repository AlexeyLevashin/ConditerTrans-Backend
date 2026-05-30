using Contracts.Auth.Requests;
using Contracts.Auth.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IAuthService
{
    Task<Result<TokensResponse>> CreateCompanyAdminAsync(Guid companyId, CreateAdminRequest request);
    Task<Result<TokensResponse>> LoginAsync(LoginRequest request);
    Task<Result<TokensResponse>> RefreshTokenAsync(string refreshToken);
    Task<Result<TokensResponse>> SetPasswordAsync(SetPasswordRequest request);
}
