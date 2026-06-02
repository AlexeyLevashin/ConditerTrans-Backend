using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.Product.Responses;
using FluentResults;
using Mapster;

namespace Application.Products;

public class ProductService(
    IProductRepository productRepository,
    ICompanyRepository companyRepository,
    ICategoryRepository categoryRepository,
    IFileServiceClient fileServiceClient) : IProductService
{
    public async Task<Result<GetProductResponse>> GetProductByIdAsync(Guid id)
    {
        var product = await productRepository.GetProductByIdAsync(id);

        if (product is null)
        {
            return Result.Fail("Продукция не найдена");
        }
        
        var response = product.DbToDetailDto();
        await ProductFileUrlEnricher.EnrichAsync([response], fileServiceClient);
        return Result.Ok(response);
    }

    public async Task<Result<List<ProductListItemResponse>>> GetAllProductsAsync(List<Guid>? companyIds, List<Guid>? categoryIds)
    {
        if (companyIds != null && companyIds.Any())
        {
            bool allCompaniesExist = await companyRepository.CheckAllExistAsync(companyIds);
            if (!allCompaniesExist)
            {
                return Result.Fail("Одна или несколько указанных компаний не найдены.");
            }
        }

        if (categoryIds != null && categoryIds.Any())
        {
            bool allCategoriesExist = await categoryRepository.CheckAllExistAsync(categoryIds);
            if (!allCategoriesExist)
            {
                return Result.Fail("Одна или несколько указанных категорий не найдены.");
            }
        }
        
        var products = await productRepository.GetAllProductsAsync(companyIds, categoryIds);
        
        var response = products.DbToListItemsDto();
        await ProductFileUrlEnricher.EnrichAsync(response, fileServiceClient);
        return Result.Ok(response);
    }

    public async Task<Result<GetProductsPagedResponse>> GetProductsPagedAsync(
        List<Guid>? companyIds,
        List<Guid>? categoryIds,
        int page,
        int pageSize)
    {
        if (companyIds != null && companyIds.Count > 0)
        {
            if (!await companyRepository.CheckAllExistAsync(companyIds))
            {
                return Result.Fail("Одна или несколько указанных компаний не найдены.");
            }
        }

        if (categoryIds != null && categoryIds.Count > 0)
        {
            if (!await categoryRepository.CheckAllExistAsync(categoryIds))
            {
                return Result.Fail("Одна или несколько указанных категорий не найдены.");
            }
        }

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var (products, totalCount) = await productRepository.GetProductsPagedAsync(
            companyIds,
            categoryIds,
            safePage,
            safePageSize);

        var items = products.DbToListItemsDto();
        await ProductFileUrlEnricher.EnrichAsync(items, fileServiceClient);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)safePageSize);

        return Result.Ok(new GetProductsPagedResponse
        {
            Result = items,
            TotalCount = totalCount,
            Page = safePage,
            PageSize = safePageSize,
            TotalPages = totalPages
        });
    }
}