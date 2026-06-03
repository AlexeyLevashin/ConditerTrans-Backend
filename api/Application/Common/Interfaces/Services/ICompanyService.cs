using Common.Enums;
using Contracts.Company.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface ICompanyService
{
    Task<Result<GetCompanyResponse?>> GetCompanyByUserIdAsync(Guid userId);
    Task<Result<GetCompanyResponse?>> GetCompanyById(Guid companyId);
    Task<Result<List<CompanyShortResponse>>> GetAllShortInfoAsync();

    Task<Result<List<CompanyShortResponse>>> GetProductionCompaniesForManagerAsync(UserRole userRole);
}
