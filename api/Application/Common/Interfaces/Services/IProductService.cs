using Contracts.Product.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IProductService
{
    Task<Result<GetProductResponse>> GetProductByIdAsync(Guid id);

    Task<Result<GetProductsPagedResponse>> GetProductsPagedAsync(
        List<Guid>? companyIds,
        List<Guid>? categoryIds,
        int page,
        int pageSize);
}
