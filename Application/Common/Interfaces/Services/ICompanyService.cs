using Contracts.Company.Responses;
using Domain.Entities;

namespace Application.Common.Interfaces.Services;

public interface ICompanyService
{
    Task<GetCompanyResponse?> GetCompanyByUserIdAsync(Guid userId);
    Task<Company?> GetCompanyById(Guid companyId);
}
