using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Products;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await context.Products
            .Include(c => c.Company)
            .Include(ct => ct.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<List<Product>> GetAllProductsAsync(List<Guid>? companyIds, List<Guid>? categoryIds)
    {
        throw new NotImplementedException();
        // IQueryable<Product> query = context.Products
        //     .AsNoTracking()
        //     .Include(c => c.Company)
        //     .Include(ct => ct.Category);
        //
        // if (companyIds is not null)
        // {
        //     query = query.Where(c => c.Id )
        // }
    }
}