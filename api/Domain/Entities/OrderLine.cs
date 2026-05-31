using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("order_lines")]
public class OrderLine
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("quantity_of_units")]
    public int QuantityOfUnits { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    public virtual Product? Product { get; set; }
    public virtual Order? Order { get; set; }
}