using Domain;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IProductRepository
{
    Task<bool> ExistsByIdAsync(Guid id);
    Task<Product?> GetProductByIdAsync(Guid id);

    Task<(List<Product> Items, int TotalCount)> GetProductsPagedAsync(
        List<Guid>? companyIds,
        List<Guid>? categoryIds,
        int page,
        int pageSize);
}
