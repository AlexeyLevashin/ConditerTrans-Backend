using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface ICategoryRepository
{
    Task<bool> CheckAllExistAsync(List<Guid> ids);
    Task<List<Category>> GetAllAsync();
}