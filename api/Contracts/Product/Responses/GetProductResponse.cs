using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Contracts.Product.Responses;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public float Quantity { get; set; }
    public float Expiry { get; set; }
    public UnitsOfMeasure UnitsOfMeasure { get; set; }
    public Guid CategoryId { get; set; }
    public Guid CompanyId { get; set; }
}