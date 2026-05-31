using Domain;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IProductRepository
{
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<List<Product>> GetAllProductsAsync(List<Guid>? companyIds, List<Guid>? categoryIds);
}