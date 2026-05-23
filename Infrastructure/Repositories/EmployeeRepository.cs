using Application.Intefaces;
using DataAccess;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    private readonly AppDbContext _context = context;
    
    public async Task AddAsync(Employee employee)
    {
        await _context.AddAsync(employee);
    }
}