using Contracts.Product.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IProductService
{
    public Task<Result<GetProductResponse>> GetProductByIdAsync(Guid id);
    public Task<Result<List<ProductListItemResponse>>> GetAllProductsAsync(List<Guid>? companyIds, List<Guid>? categoryIds);
}