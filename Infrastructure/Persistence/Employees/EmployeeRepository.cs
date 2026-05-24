using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain.Entities;

namespace Infrastructure.Persistence.Employees;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public async Task AddAsync(Employee employee)
    {
        await context.AddAsync(employee);
    }
}