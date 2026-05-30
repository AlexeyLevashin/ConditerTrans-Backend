using Contracts.Product.Responses;

namespace Application.Common.Interfaces.Services;

public interface IProductService
{
    public Task<GetProductResponse?> GetProductByIdAsync(Guid id);
    public Task<List<GetProductResponse>> GetAllProductsAsync(List<Guid>? companyIds, List<Guid>? categoryIds);
}