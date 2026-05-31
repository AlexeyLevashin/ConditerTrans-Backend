using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Categories;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<bool> CheckAllExistAsync(List<Guid> ids)
    {
        var uniqueIds = ids.Distinct().ToList();

        var existingCount = await context.Categories
            .Where(c => uniqueIds.Contains(c.Id))
            .CountAsync();

        return existingCount == uniqueIds.Count;
    }
}