using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface ICategoryRepository
{
    Task<bool> CheckAllExistAsync(List<Guid> ids);
    public Task<List<Category>> GetAllAsync();
}