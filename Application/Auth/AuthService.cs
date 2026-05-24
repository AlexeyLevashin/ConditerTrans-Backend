using Application.Intefaces;
using Application.Intefaces.Authentication;
using Contracts.Auth.Requests;
using Contracts.Auth.Responses;
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
    public async Task<Result<CreateCompanyAdminResponse>> CreateCompanyAdmin(
        Guid companyId,
        CreateAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var company = await companyRepository.GetByIdAsync(companyId);

        if (company is null)
        {
            return Result.Fail("Компания не найдена");
        }

        var employee = AdminRegistrationMapper.ToEmployee(companyId, request);
        var passwordHash = passwordService.Hash(request.Password);
        var user = AdminRegistrationMapper.ToUser(request, employee, passwordHash, company.CompanyType);

        await employeeRepository.AddAsync(employee);
        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new CreateCompanyAdminResponse
        {
            AccessToken = jwtProvider.GenerateAccessToken(user),
            RefreshToken = jwtProvider.GenerateRefreshToken(user)
        });
    }
}
