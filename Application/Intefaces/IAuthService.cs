using Contracts.Auth.Requests;
using Contracts.Auth.Responses;
using FluentResults;

namespace Application.Intefaces;

public interface IAuthService
{
    Task<Result<CreateCompanyAdminResponse>> CreateCompanyAdmin(
        Guid companyId,
        CreateAdminRequest request,
        CancellationToken cancellationToken = default);
}
