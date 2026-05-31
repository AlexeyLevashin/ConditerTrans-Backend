using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface ICompanyRepository
{
    Task AddAsync(Company company);
    Task<Company?> GetByIdAsync(Guid companyId);
    Task<Company?> GetByUserIdAsync(Guid userId);
    Task<bool> GetAdminInCompanyExistAsync(Guid companyId);
    Task<bool> CheckAllExistAsync(List<Guid> ids);
    Task<List<Company>> GetAllAsync();
}