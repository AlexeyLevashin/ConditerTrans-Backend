using Domain.Entities;

namespace Application.Intefaces;

public interface ICompanyRepository
{
    public Task AddAsync(Company company);
    public Task<Company?> GetByIdAsync(Guid companyId);
}