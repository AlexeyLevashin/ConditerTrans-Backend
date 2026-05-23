using Domain.Entities;

namespace Application.Intefaces;

public interface IEmployeeRepository
{
    public Task AddAsync(Employee employee);
}