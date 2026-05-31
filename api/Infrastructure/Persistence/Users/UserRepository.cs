using Application.Common.Interfaces.Persistence.Repositories;
using Common.Enums;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Users;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task AddAsync(User user)
    {
        await context.AddAsync(user);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> GetByIdAsync(Guid userId)
    {
        return context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<User>> GetByCompanyIdAsync(Guid companyId)
    {
        return await context.Users
            .Include(u => u.Employee)
            .Where(u => u.Employee != null && u.Employee.CompanyId == companyId)
            .OrderBy(u => u.Employee!.Surname)
            .ThenBy(u => u.Employee!.Name)
            .ToListAsync();
    }

    public async Task<List<User>> GetDriversByCompanyIdAsync(Guid companyId)
    {
        return await context.Users
            .Include(u => u.Employee)
            .Where(u =>
                u.UserRole == UserRole.Driver
                && u.Employee != null
                && u.Employee.CompanyId == companyId)
            .OrderBy(u => u.Employee!.Surname)
            .ThenBy(u => u.Employee!.Name)
            .ToListAsync();
    }

    public Task<User?> GetDriverByIdAndCompanyIdAsync(Guid driverId, Guid companyId)
    {
        return context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u =>
                u.Id == driverId
                && u.UserRole == UserRole.Driver
                && u.Employee != null
                && u.Employee.CompanyId == companyId);
    }
}