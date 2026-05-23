using Common.Enums;

namespace Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public int NomenclatureNumber { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public double Quantity { get; set; }
    public int ExpirationDays { get; set; }
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; }
    public Guid CompanyId { get; set; }
    public virtual Company Company { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public virtual UnitOfMeasure UnitOfMeasure { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}