using System.Globalization;
using Common.Enums;
using Contracts.Categories.Responses;
using Contracts.Company.Responses;
using Contracts.Product.Responses;
using Domain;

namespace Application.Products;

public static class ProductMapper
{
    public static GetProductResponse DbToDetailDto(this Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Expiry = product.Expiry,
        FormattedQuantity = GetFormattedQuantity(product.Quantity, product.UnitsOfMeasure),
        FileId = product.FileId,

        Company = product.Company != null
            ? new CompanyShortResponse
            {
                Id = product.Company.Id,
                Name = product.Company.Name
            } : null,

        Category = product.Category != null
            ? new GetCategoryResponse
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            } : null
    };
    
    private static ProductListItemResponse DbToListItemDto(this Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        FormattedQuantity = GetFormattedQuantity(product.Quantity, product.UnitsOfMeasure),
        FileId = product.FileId,

        Company = product.Company != null
            ? new CompanyShortResponse
            {
                Id = product.Company.Id,
                Name = product.Company.Name
            } : null,

        Category = product.Category != null
            ? new GetCategoryResponse
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            } : null
    };

    public static List<ProductListItemResponse> DbToListItemsDto(this List<Product> products)
    {
        return products.Select(p => p.DbToListItemDto()).ToList();
    }
    
    private static string GetFormattedQuantity(float quantity, UnitsOfMeasure unit)
    {
        var culture = CultureInfo.InvariantCulture;

        return unit switch
        {
            UnitsOfMeasure.Pieces => quantity.ToString(culture) + " шт",
        
            UnitsOfMeasure.Grams => quantity.ToString(culture) + " г",
            
            UnitsOfMeasure.Milliliters => quantity.ToString(culture) + " мл",
            
            _ => quantity.ToString(culture)
        };
    }
}