using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;
using Contracts.Categories.Responses;
using Contracts.Company.Responses;

namespace Contracts.Product.Responses;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public float Expiry { get; set; }
    public string FormattedQuantity { get; set; } = null!;
    public CompanyShortResponse? Company { get; set; }
    public GetCategoryResponse? Category { get; set; }
}