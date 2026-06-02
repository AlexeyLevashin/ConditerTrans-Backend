using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid userId);
    Task<List<User>> GetByCompanyIdAsync(Guid companyId);
    Task<List<User>> GetDriversByCompanyIdAsync(Guid companyId);
    Task<User?> GetDriverByIdAndCompanyIdAsync(Guid driverId, Guid companyId);
    Task<User?> GetDriverByEmployeeIdAsync(Guid employeeId);
}