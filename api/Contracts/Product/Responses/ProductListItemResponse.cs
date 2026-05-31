using Common.Enums;
using Contracts.Categories.Responses;
using Contracts.Company.Responses;

namespace Contracts.Product.Responses;

public class ProductListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string FormattedQuantity { get; set; } = null!;
    public CompanyShortResponse? Company { get; set; }
    public GetCategoryResponse? Category { get; set; }
}