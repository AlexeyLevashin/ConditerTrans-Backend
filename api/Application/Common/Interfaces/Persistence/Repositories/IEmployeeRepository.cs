using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IEmployeeRepository
{
    Task AddAsync(Employee employee);
    Task<Employee?> GetByPhoneAsync(string phone);
    Task<Employee?> GetByIdAsync(Guid employeeId);
}