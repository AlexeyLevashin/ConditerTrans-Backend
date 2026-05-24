using Application.Intefaces;
using DataAccess;

namespace Infrastructure.Repositories;

public class UnitOfWork(AppDbContext appDbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return appDbContext.SaveChangesAsync(cancellationToken);
    }
}