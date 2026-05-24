using Application.Common.Interfaces;
using Application.Intefaces;
using Contracts.Company.Responses;
using Domain.Entities;

namespace Application.Companies;

public class CompanyService(ICompanyRepository companyRepository) : ICompanyService
{
    public async Task<GetCompanyResponse?> GetCompanyByUserIdAsync(Guid userId)
    {
        var company = await companyRepository.GetByUserIdAsync(userId);
        return company is null ? null : CompanyMapper.ToResponse(company);
    }

    public Task<Company?> GetCompanyById(Guid companyId)
    {
        return companyRepository.GetByIdAsync(companyId);
    }
}
