using Contracts.Company.Responses;
using Domain.Entities;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface ICompanyService
{
    public Task<Result<GetCompanyResponse?>> GetCompanyByUserIdAsync(Guid userId);
    public Task<Result<GetCompanyResponse?>> GetCompanyById(Guid companyId);
    public Task<Result<List<CompanyShortResponse>>> GetAllShortInfoAsync();
}
