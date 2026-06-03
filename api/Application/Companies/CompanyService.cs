using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Company.Responses;
using FluentResults;
using Mapster;

namespace Application.Companies;

public class CompanyService(ICompanyRepository companyRepository) : ICompanyService
{
    private const string ManagerOnlyError = "Доступно только менеджеру по закупкам";

    public async Task<Result<GetCompanyResponse?>> GetCompanyByUserIdAsync(Guid userId)
    {
        var company = await companyRepository.GetByUserIdAsync(userId);

        if (company is null)
        {
            return Result.Fail("Компания пользователя не найдена");
        }

        return company.Adapt<GetCompanyResponse>();
    }

    public async Task<Result<GetCompanyResponse?>> GetCompanyById(Guid companyId)
    {
        var company = await companyRepository.GetByIdAsync(companyId);

        if (company is null)
        {
            return Result.Fail("Компания не найдена");
        }

        return company.Adapt<GetCompanyResponse>();
    }
    
    public async Task<Result<List<CompanyShortResponse>>> GetAllShortInfoAsync()
    {
        var results = await companyRepository.GetAllAsync();
        return results.Adapt<List<CompanyShortResponse>>();
    }

    public async Task<Result<List<CompanyShortResponse>>> GetProductionCompaniesForManagerAsync(UserRole userRole)
    {
        if (userRole != UserRole.Manager)
        {
            return Result.Fail(ManagerOnlyError);
        }

        var companies = await companyRepository.GetByCompanyTypeAsync(CompanyType.ProductionDispatcher);
        return Result.Ok(companies.Adapt<List<CompanyShortResponse>>());
    }
}
