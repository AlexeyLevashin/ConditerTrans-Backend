using Application.Intefaces;
using DataAccess;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public async Task AddAsync(Employee employee)
    {
        await context.AddAsync(employee);
    }
}