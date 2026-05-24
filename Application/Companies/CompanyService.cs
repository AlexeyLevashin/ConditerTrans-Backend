using Application.Common.Interfaces;
using Application.Intefaces;
using Domain.Entities;

namespace Application.Companies;

public class CompanyService : ICompanyService
{
    private ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public Task<Company?> GetCompanyById(Guid companyId)
    {
        return _companyRepository.GetByIdAsync(companyId);
    }
}