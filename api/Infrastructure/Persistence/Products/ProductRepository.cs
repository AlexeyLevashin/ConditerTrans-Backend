using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Products;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public Task<bool> ExistsByIdAsync(Guid id)
    {
        return context.Products.AnyAsync(p => p.Id == id);
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await context.Products
            .Include(c => c.Company)
            .Include(ct => ct.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetAllProductsAsync(List<Guid>? companyIds, List<Guid>? categoryIds)
    {
        IQueryable<Product> query = context.Products
            .AsNoTracking()
            .Include(c => c.Company)
            .Include(ct => ct.Category);
        
        if (companyIds != null && companyIds.Any())
        {
            query = query.Where(c => companyIds.Contains(c.CompanyId));
        }

        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(ct => categoryIds.Contains(ct.CategoryId));
        }

        return await query.ToListAsync();
    }

    public async Task<(List<Product> Items, int TotalCount)> GetProductsPagedAsync(
        List<Guid>? companyIds,
        List<Guid>? categoryIds,
        int page,
        int pageSize)
    {
        IQueryable<Product> query = context.Products
            .AsNoTracking()
            .Include(c => c.Company)
            .Include(ct => ct.Category);

        if (companyIds != null && companyIds.Count > 0)
        {
            query = query.Where(c => companyIds.Contains(c.CompanyId));
        }

        if (categoryIds != null && categoryIds.Count > 0)
        {
            query = query.Where(ct => categoryIds.Contains(ct.CategoryId));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}