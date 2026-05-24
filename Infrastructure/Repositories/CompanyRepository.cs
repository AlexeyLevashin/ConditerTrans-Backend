using Application.Intefaces;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

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
}