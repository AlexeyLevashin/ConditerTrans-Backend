using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Common.Enums;
using Domain.Entities;

namespace Domain;

[Table("products")]
public class Product
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("price")]
    public decimal Price { get; set; }
    
    [Column("quantity")]
    public float Quantity { get; set; }
    
    [Column("expiry")]
    public float Expiry { get; set; }
    
    [Column("units_of_measure")]
    public UnitsOfMeasure UnitsOfMeasure { get; set; }
    
    [Column("category_id")]
    public Guid CategoryId { get; set; }
    
    [Column("company_id")]
    public Guid CompanyId { get; set; }
    
    public virtual Company Company { get; set; }
    public virtual Category Category { get; set; }
}