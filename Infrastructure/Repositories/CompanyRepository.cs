using Application.Intefaces;
using DataAccess;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class CompanyRepository(AppDbContext context) : ICompanyRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddAsync(Company company)
    {
        await _context.AddAsync(company);
    }
}