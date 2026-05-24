using System.Security.Claims;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.Auth.Requests;
using Contracts.Auth.Responses;
using Domain.Entities;
using FluentResults;

namespace Application.Auth;

public class AuthService(
    ICompanyRepository companyRepository,
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    IJwtProvider jwtProvider) : IAuthService
{
    private const string LoginFailureError = "Неверный логин или пароль";
    private const string ParseTokenFailureError = "Невалидный refresh token";

    public async Task<Result<TokensResponse>> CreateCompanyAdminAsync(Guid companyId, CreateAdminRequest request)
    {
        var company = await companyRepository.GetByIdAsync(companyId);

        if (company is null)
        {
            return Result.Fail("Компания не найдена");
        }

        var adminCompanyIsExist = await companyRepository.GetAdminInCompanyExistAsync(companyId);

        if (adminCompanyIsExist)
        {
            return Result.Fail("В данной компании уже есть администратор");
        }

        var employee = AuthMapper.ToEmployee(companyId, request);
        var passwordHash = passwordService.Hash(request.Password);
        var user = AuthMapper.ToUser(request, employee, passwordHash, company.CompanyType);

        await employeeRepository.AddAsync(employee);
        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return Result.Ok(GetTokensResponseByUser(user));
    }

    public async Task<Result<TokensResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);

        if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
        {
            return Result.Fail(LoginFailureError);
        }
        
        return Result.Ok(GetTokensResponseByUser(user));
    }

    public async Task<Result<TokensResponse>> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Fail(ParseTokenFailureError);
        }

        if (!jwtProvider.TryGetClaims(refreshToken, out var principal) || principal is null)
        {
            return Result.Fail(ParseTokenFailureError);
        }

        var userIdValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Result.Fail(ParseTokenFailureError);
        }

        var user = await userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        return Result.Ok(GetTokensResponseByUser(user));
    }

    private TokensResponse GetTokensResponseByUser(User user) => new()
    {
        AccessToken = jwtProvider.GenerateAccessToken(user),
        RefreshToken = jwtProvider.GenerateRefreshToken(user)
    };
}
