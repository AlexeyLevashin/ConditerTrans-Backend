using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Employees;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public async Task AddAsync(Employee employee)
    {
        await context.AddAsync(employee);
    }
    
    public async Task<Employee?> GetByPhoneAsync(string phone)
    {
        return await context.Employees.FirstOrDefaultAsync(u => u.Phone == phone);
    }
}