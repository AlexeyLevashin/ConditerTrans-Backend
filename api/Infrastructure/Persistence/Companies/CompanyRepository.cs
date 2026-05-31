using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Companies;

public class CompanyRepository(AppDbContext context) : ICompanyRepository
{
    public async Task AddAsync(Company company)
    {
        await context.AddAsync(company);
    }

    public Task<Company?> GetByIdAsync(Guid companyId)
    {
        return context.Companies.FirstOrDefaultAsync(company => company.Id == companyId);
    }

    public Task<Company?> GetByUserIdAsync(Guid userId)
    {
        return context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Employee!.Company)
            .FirstOrDefaultAsync();
    }

    public Task<bool> GetAdminInCompanyExistAsync(Guid companyId)
    {
        return context.Users
            .AnyAsync(u => u.IsAdmin && u.Employee!.CompanyId == companyId);
    }
    
    public async Task<bool> CheckAllExistAsync(List<Guid> ids)
    {
        var uniqueIds = ids.Distinct().ToList();

        var existingCount = await context.Companies
            .Where(c => uniqueIds.Contains(c.Id))
            .CountAsync();

        return existingCount == uniqueIds.Count;
    }
}