using Contracts.Company.Responses;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICompanyService
{
    Task<GetCompanyResponse?> GetCompanyByUserIdAsync(Guid userId);
    Task<Company?> GetCompanyById(Guid companyId);
}
