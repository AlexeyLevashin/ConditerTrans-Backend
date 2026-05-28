using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IEmployeeRepository
{
    public Task AddAsync(Employee employee);
    public Task<Employee?> GetByPhoneAsync(string phone);
}