using Domain;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IProductRepository
{
    public Task<Product?> GetProductByIdAsync(Guid id);
    public Task<List<Product>> GetAllProductsAsync(Guid? companyId, Guid? categoryId);
}