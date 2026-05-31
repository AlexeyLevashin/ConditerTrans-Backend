using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.Product.Responses;
using FluentResults;
using Mapster;

namespace Application.Products;

public class ProductService(IProductRepository productRepository, ICompanyRepository companyRepository, ICategoryRepository categoryRepository) : IProductService
{
    public async Task<Result<GetProductResponse>> GetProductByIdAsync(Guid id)
    {
        var product = await productRepository.GetProductByIdAsync(id);

        if (product is null)
        {
            return Result.Fail("Продукция не найдена");
        }
        
        return Result.Ok(product.DbToDetailDto());
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
        
        return Result.Ok(products.DbToListItemsDto());
    }
}