using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.Product.Responses;
using FluentResults;
using Mapster;

namespace Application.Products;

public class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<GetProductResponse?> GetProductByIdAsync(Guid id)
    {
        var product = await productRepository.GetProductByIdAsync(id);

        if (product is null)
        {
            Result.Fail("Продукт не найден");
        }
        
        var result = product.Adapt<GetProductResponse>();
        return result;
    }

    public Task<List<GetProductResponse>> GetAllProductsAsync(List<Guid>? companyIds, List<Guid>? categoryIds)
    {
        throw new NotImplementedException();
    }
}